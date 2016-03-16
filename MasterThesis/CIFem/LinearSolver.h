#pragma once

#include "CIFem_dll.h"
#include "Structure.h"
#include <memory>
#include "ISolver.h"
#include "Vector3d.h"
#include "LoadCombination.h"

typedef std::set<std::shared_ptr<CIFem::DOF>> DofSet;

namespace CIFem
{
	class CIFEM_API LinearSolver : public CIFem::ISolver
	{
		bool _gravityOn;
		Vector3d _gravityField;
		std::vector<LoadCombination> _loadCobms;

	public:
		LinearSolver();
		LinearSolver(std::shared_ptr<CIFem::Structure> structure);
		LinearSolver(std::shared_ptr<CIFem::Structure> structure, bool applyGravity);
		LinearSolver(std::shared_ptr<CIFem::Structure> structure, Vector3d gravityDirection);
		~LinearSolver();

		void Solve();


	private:

		void ApplyGravityToElements();
		void ApplyNodalForces();

		arma::colvec GetForceVector(DofSet &);
		arma::colvec GetDisplacementVector(DofSet &);

		void LinEqSolve(arma::mat & K, arma::colvec & a, arma::colvec & f, arma::mat & C, DofSet & spDofs, arma::colvec & s);
	};
}

