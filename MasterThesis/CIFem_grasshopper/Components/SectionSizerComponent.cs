﻿using CIFem_wrapper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIFem_grasshopper
{
    public class SectionSizerComponent : GH_Component
    {
        public List<string> log { get; set; }
        private List<ResultElement> resElems { get; set; }

        public SectionSizerComponent(): base("Section Sizer", "SecSize", "A simple section sizer based on a linear solver.", "CIFem", "Optimizers")
        {
            log = new List<string>();
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("f931cdf0-1eef-4a98-885e-899b7b55b3fe");
            }
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new StructureParam(), "Structure", "Str", "The structure to solve", GH_ParamAccess.item);
            pManager.AddParameter(new LoadCombParameter(), "Load Combinations", "LC", "Diffrent loadcombinations for the structure", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Solve", "Go", "Toggle the solver", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Max Iterations", "MI", "Maximum iterations to run before brake", GH_ParamAccess.item, 100);
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
            List<WR_LoadCombination> loadCombinations = new List<WR_LoadCombination>();
            bool go = false;
            int maxIter = 0;

            if (!DA.GetData(0, ref structure)) { return; }
            if (!DA.GetDataList(1, loadCombinations)) { return; }
            if (!DA.GetData(2, ref go)) { return; }
            if(!DA.GetData(3, ref maxIter)) { return; }


            if (go)
            {
                resElems = new List<ResultElement>();

                // Solve
                WR_StandardSectionSizer secSizer = new WR_StandardSectionSizer(structure);

                foreach (WR_LoadCombination lc in loadCombinations)
                {
                    secSizer.AddLoadCombination(lc);
                }

                secSizer.Run(maxIter);


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

