#pragma once
#include "CIFem_dll.h"
#include <vector>
#include "DofRelease.h"

namespace CIFem
{
	class CIFEM_API IRelease
	{
	public:
		~IRelease();
		IRelease(std::vector<DofRelease>);
		std::vector<DofRelease> GetReleases();

	protected:
		IRelease();
		void SetReleases(std::vector<DofRelease>);
		virtual int GetNumberOfReleases() = 0;		// Sets number of releases, used in order to avoid coding errors
		



	private:
		std::vector<DofRelease> _releases;
	};
}
