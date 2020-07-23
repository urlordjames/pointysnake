using System.Collections.Generic;

namespace makebin
{
    class BranchManager
    {
        Dictionary<int, int> branches = new Dictionary<int, int>();
        Dictionary<int, string> branchtype = new Dictionary<int, string>();

        public void pushbranch(int id, int instruction, string type) {
            branches.Add(id, instruction);
            branchtype.Add(id, type);
        }

        public int getbranch(int id) {
            return branches[id];
        }

        public string getbranchtype(int id) {
            return branchtype[id];
        }
    }
}
