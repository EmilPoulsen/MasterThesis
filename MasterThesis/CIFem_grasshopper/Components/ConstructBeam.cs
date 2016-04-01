﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CIFem_wrapper;

using Grasshopper;
using Grasshopper.Kernel;

namespace CIFem_grasshopper
{
    public class ConstructBeam : GH_Component
    {
        public ConstructBeam() : base("ConstructBeam Beam", "Beam", "Constructs a structual beam", "CIFem", "Elements")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("c9293b13-491a-4e3a-b4bf-271a8d1ec4a6");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Centre Line", "CL", "Centre line of the beam", GH_ParamAccess.item);
            pManager.AddParameter(new BeamPropertiesParam(), "Properties", "P", "Properties for the beam", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "N", "Normal of the beam", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BeamParam(), "Beam", "B", "The constructed beam", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BeamProperties prop = null;
            Rhino.Geometry.Line ln = Rhino.Geometry.Line.Unset;
            Rhino.Geometry.Vector3d norm = Rhino.Geometry.Vector3d.Unset;

            if (!DA.GetData(0, ref ln)) { return; }
            if (!DA.GetData(1, ref prop)) { return; }
            if (!DA.GetData(2, ref norm)) { return; }

            norm.Unitize();

            double factor = Utilities.GetScalingFactorFromRhino();

            WR_Vector wrNorm = new WR_Vector(norm.X, norm.Y, norm.Z);
            WR_XYZ st = new WR_XYZ(ln.FromX* factor, ln.FromY* factor, ln.FromZ* factor);
            WR_XYZ en = new WR_XYZ(ln.ToX* factor, ln.ToY* factor, ln.ToZ* factor);

            WR_Elem3dRcp beam = new WR_Elem3dRcp(st, en, prop.StartRelease, prop.EndRelease, prop.CrossSection, prop.Material, wrNorm, prop.SectionGroup);
            
            DA.SetData(0, beam);

        }
    }
}
