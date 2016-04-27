﻿using CIFem_wrapper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIFem_grasshopper
{
    public class ModeShapeOptimizerComponentModes : GH_Component
    {
        public List<string> log { get; set; }
        private List<ResultElement> resElems { get; set; }

        public ModeShapeOptimizerComponentModes(): base("Mode shape optimizer", "ModeSize", "Section sizer sizing elements based on mode shapes", "CIFem", "Optimizers")
        {
            log = new List<string>();
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("3df875da-f58a-4bb7-8b64-346c24d1233b");
            }
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new StructureParam(), "Structure", "Str", "The structure to solve", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Solve", "Go", "Toggle the solver", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Modes", "m", "The modes to optimize for", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scale factor", "sFac", "Optional scale factor. Scales all forces based on this value. Deafaults to 1", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Add displacements, reactions, element forces etc

            pManager.AddTextParameter("Messageboard", "log", "Outputs a log of the performed calculation", GH_ParamAccess.list);

            pManager.AddParameter(new ResultElementParam(), "Result Elements", "RE", "Result elements, storing results from the calculation", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Indata
            WR_Structure structure = null;
            bool go = false;
            List<int> modes = new List<int>();
            double sFac = 0;

            if (!DA.GetData(0, ref structure)) { return; }
            if (!DA.GetData(1, ref go)) { return; }
            if (!DA.GetDataList(2, modes)) { return; }
            if (!DA.GetData(3, ref sFac)) { return; }


            if (go)
            {
                resElems = new List<ResultElement>();

                // Solve
                WR_ModeShapeOptimizer optimizer = new WR_ModeShapeOptimizer(structure);

                optimizer.Run(modes, sFac);

                // Extract results
                List<WR_IElement> elems = structure.GetAllElements();
                for (int i = 0; i < elems.Count; i++)
                {

                    if (elems[i] is WR_Element3d)
                    {
                        WR_Element3d el3d = (WR_Element3d)elems[i];
                        ResultElement re = new ResultElement(el3d);
                        resElems.Add(re);
                    }
                }
            }
            DA.SetDataList(0, log);
            DA.SetDataList(1, resElems);

        }
    }
}
