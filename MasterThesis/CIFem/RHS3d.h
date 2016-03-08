#pragma once

#include "ICrossSection.h"
#include <math.h>
#include <stdexcept>

namespace CIFem
{
	class CIFEM_API RHS3d : public ICrossSection
	{
		double _height;
		double _width;
		double _thickness;
	public:
		RHS3d();
		RHS3d(double height, double width, double thickness);
		~RHS3d();

	private:
		void CalcSectionProperties();
	};
}

