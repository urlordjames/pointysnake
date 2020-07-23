using System.Collections.Generic;

namespace makebin
{
    class BranchManager
    {
        public Dictionary<int, int> branches = new Dictionary<int, int>();

        public void pushbranch(int id, int instruction) {
            branches.Add(id, instruction);
        }

        public int getbranch(int id) {
            return branches[id];
        }
    }
}
