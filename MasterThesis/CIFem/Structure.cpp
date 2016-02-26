#include "Structure.h"

CIFem::Structure::Structure()
{
}


CIFem::Structure::~Structure()
{
	for (int i = 0; i < _elements.size(); i++)
		delete _elements[i];

	_elements.clear();
}

void CIFem::Structure::AddNode(INode * node)
{
	_nodes.push_back(node);
}

void CIFem::Structure::AddNode(std::vector<INode *> nodes)
{
	for (int i = 0; i < nodes.size(); i++)
		AddNode(nodes[i]);
}

void CIFem::Structure::AddElementRcp(IElementRcp * elementRcp)
{
	_elementRcps.push_back(elementRcp);
}

void CIFem::Structure::AddElementRcp(std::vector<IElementRcp *> elements)
{
	for (int i = 0; i < elements.size(); i++)
		AddElementRcp(elements[i]);
}

void CIFem::Structure::AddDisplacementRestraint(DisplacementRestraint dispRest)
{
	_displacementRestraints.push_back(dispRest);
}

void CIFem::Structure::Solve()
{
	// Build structure
	BuildStructure();

	// Get dofs
	std::vector<std::shared_ptr<CIFem::DOF>> spDofs = GetDofs(_nodes);

	// Update dof kIndex
	SetDofKMatIndex(spDofs);

	// Create K matrix
	arma::sp_mat K = AssembleStiffnessMatrix(spDofs);
	
	// Get a and f vectors 
														// Get transformation vector for restraints
	arma::colvec a = GetDisplacementVector(spDofs);		// Get displacement vector
	arma::colvec f = GetForceVector(spDofs);			// Get force vector

	// Transform matrices and vectors to allow for non-global restraints

	// Solve K matrix
	LinEqSolve(K, a, f, spDofs);

	// Store results in dofs
	StoreResultsInDofs(a, f, spDofs);
}


std::vector<CIFem::IElement *> CIFem::Structure::CreateElements(
	std::vector<IElementRcp *> elRcps, std::vector<INode *> nodes)
{
	std::vector<IElement *> elements;
	std::vector<IElement *> outElems;
	for (int i = 0; i < elRcps.size(); i++)
	{
		outElems.clear();
		outElems = elRcps[i]->CreateElement(nodes);

		for (int j = 0; j < outElems.size(); j++)
			elements.push_back(outElems[j]);
	}

	return elements;
}

void CIFem::Structure::BuildStructure()
{
	// Create elements from recipes and assign
	_elements = CreateElements(_elementRcps, _nodes);
}

std::vector<std::shared_ptr<CIFem::DOF>> CIFem::Structure::GetDofs(std::vector<INode *> nodes)
{
	std::vector<std::shared_ptr<CIFem::DOF>> dofs;
	for (int i = 0; i < nodes.size(); i++)
	{
		std::vector<std::shared_ptr<CIFem::DOF>> nDofs = nodes[i]->GetDofs();
		for (int j = 0; j < nDofs.size(); j++)
			dofs.push_back(nDofs[j]);
	}

	return dofs;
}


void CIFem::Structure::SetDofKMatIndex(std::vector<std::shared_ptr<CIFem::DOF>> spDofs)
{
	//Counting dofs, assuming all dofs exists in the nodes
	int dofCounter = 0;
	for each (std::shared_ptr<DOF> dof in spDofs)
	{
		dof->_kIndex = dofCounter;
		dofCounter++;
	}
}

arma::sp_mat CIFem::Structure::AssembleStiffnessMatrix(std::vector<std::shared_ptr<CIFem::DOF>> spDofs)
{
	// Count dofs
	int dofCount = spDofs.size();
	
	// Create K matrix
	arma::sp_mat K(dofCount, dofCount);

	// Assemble K matrix
	for each (IElement * pElem in _elements)
	{
		arma::mat Ke = pElem->GetStiffnessMatrix();

		AssembleElementsInKMat(K, Ke, pElem->GetDofs());
	}
	
	return K;
}

void CIFem::Structure::AssembleElementsInKMat(arma::sp_mat & K, arma::mat & Ke, std::vector<std::shared_ptr<DOF>> spEDofs)
{
	//DEBUG
	K.print("K:");
	Ke.print("Ke");

	// Check inputs
	int n = K.n_rows;
	for each (std::shared_ptr<DOF> spDof in spEDofs)
		if (spDof->_kIndex > n - 1)
			throw std::invalid_argument("Error in k matrix numbering");

	int nKe = Ke.n_rows;
	if (nKe != spEDofs.size())
		throw std::invalid_argument("Element dofs and Ke matrix rows mismatch!");

	// Assemble K matrix
	for (int i = 0; i < nKe; i++)
	{
		int iKIndex = spEDofs[i]->_kIndex;
		for (int j = 0; j < nKe; j++)
		{
			int jKIndex = spEDofs[j]->_kIndex;
			K.at(iKIndex, jKIndex) = Ke.at(i, j);
		}
	}
}

arma::colvec CIFem::Structure::GetForceVector(std::vector<std::shared_ptr<DOF>> spDofs)
{
	arma::colvec f(spDofs.size(), arma::fill::zeros);

	for each (std::shared_ptr<DOF> spDof in spDofs)
		f(spDof->_kIndex) = spDof->_f;

	return f;
}

arma::colvec CIFem::Structure::GetDisplacementVector(std::vector<std::shared_ptr<DOF>> spDofs)
{
	arma::colvec a(spDofs.size(), arma::fill::zeros);

	for each (std::shared_ptr<DOF> spDof in spDofs)
		a(spDof->_kIndex) = spDof->GetTranslation();

	return a;
}

void CIFem::Structure::LinEqSolve(
	arma::sp_mat & K, arma::colvec & a, arma::colvec & f, std::vector<std::shared_ptr<DOF>> spDofs)
{
	// Check prescribed deformations
	std::vector<unsigned int> fDof;
	std::vector<unsigned int> pDof;
	for each (std::shared_ptr<DOF> dof in spDofs)
	{
		if (dof->HasSetTranslation())
			pDof.push_back(dof->_kIndex);
		else
			fDof.push_back(dof->_kIndex);
	}

	// There must be a way to do this from the start,
	// however, I think uvec is fixed size
	arma::uvec ufDof(fDof);
	arma::uvec upDof(pDof);

	arma::colvec fa(fDof.size());

	// Solve deformations
	arma::mat Kmat(K); // Debugging, workaround this to improve speed
	arma::sp_mat Kff(Kmat(ufDof, ufDof));
	arma::mat f_fsolved = f(ufDof)- Kmat(ufDof, upDof) * a(upDof);
	fa = arma::spsolve(Kff, f_fsolved);

	a(ufDof) = fa;

	// Solve forces
	f = K*a;
}

void CIFem::Structure::StoreResultsInDofs(arma::colvec a, arma::colvec f, std::vector<std::shared_ptr<DOF>> spDofs)
{
	// Set results in dofs
	for (int i = 0; i < spDofs.size(); i++)
	{
		spDofs[i]->_resA = a(spDofs[i]->_kIndex);
		spDofs[i]->_resF = f(spDofs[i]->_kIndex);
	}
}
