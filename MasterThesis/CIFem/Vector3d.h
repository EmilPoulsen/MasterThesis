#pragma once
#include "CIFem_dll.h"
#include <math.h>
#include "XYZ.h"
#include <vector>

namespace CIFem
{

	class CIFEM_API Vector3d
	{
		double _x, _y, _z;

	public:
		Vector3d();
		Vector3d(double x, double y, double z);
		Vector3d(double x, double y, double z, bool unitize);
		Vector3d(XYZ st, XYZ en);
		double GetX() const;
		double GetY() const;
		double GetZ() const;
		double GetLength() const;
		double DotProduct(Vector3d other);
		std::vector<double> ToStandardVector() const;
		void Unitize();
		~Vector3d();

		//Operators
		Vector3d operator * (double t);
		Vector3d operator - (Vector3d other);
		Vector3d operator + (Vector3d other);

		//Static vector setups
		static Vector3d ZeroVec();

		static Vector3d X();
		static Vector3d Y();
		static Vector3d Z();

		static Vector3d CrossProduct(Vector3d a, Vector3d b);
	};

}