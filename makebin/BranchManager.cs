using System.Collections.Generic;

namespace makebin
{
    class BranchManager
    {
        Dictionary<int, int> branches = new Dictionary<int, int>();
        Dictionary<int, string> branchtype = new Dictionary<int, string>();

        public void pushBranch(int id, int instruction, string type) {
            branches.Add(id, instruction);
            branchtype.Add(id, type);
        }

        public int getBranch(int id) {
            return branches[id];
        }

        public string getBranchType(int id) {
            return branchtype[id];
        }

        public bool branchExists(int id) {
            return branches.ContainsKey(id);
        }
    }
}
