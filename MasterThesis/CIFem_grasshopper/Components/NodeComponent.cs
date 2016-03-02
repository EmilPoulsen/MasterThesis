﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

using CIFem_wrapper;

namespace CIFem_grasshopper
{
    public class NodeComponent : GH_Component
    {
        public NodeComponent(): base("RestraintNode", "RNode", "Creates restraint nodes", "CIFem", "Nodes")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("e946fa47-6943-497a-896d-a51949f33377");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "P", "Node position", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Releases", "R", "Releases for the restraint. Should be a list of 6 bools. True = Fixed, False = Free", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Orientation", "Pl", "Add an optional plane for node restraints", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new NodeParam(), "RestraintNode", "RN", "Restraint nodes", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///// INPUTS /////
            Rhino.Geometry.Point3d pt = Rhino.Geometry.Point3d.Unset;
            List<bool> rels = null;
            Rhino.Geometry.Plane pl = Rhino.Geometry.Plane.Unset;

            if (!DA.GetData(0, ref pt)) { return; }

            if (!DA.GetData(1, ref rels)) { return; }
            if (rels.Count != 6)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of bools in input should be 6");
                return;
            }

            if (!DA.GetData(2, ref pl))
            {
                // If no plane submitted, use global XY plane
                pl = new Rhino.Geometry.Plane(new Rhino.Geometry.Point3d(0,0,0), 
                    new Rhino.Geometry.Vector3d(1,0,0), new Rhino.Geometry.Vector3d(0,1,0));
            }


            ///// SOLVE /////

            WR_INode node = new WR_Node3d();


            ///// OUTPUTS /////

        }
    }
}
