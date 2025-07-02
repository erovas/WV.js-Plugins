using SystemMenu.Win32.Enums;

namespace SystemMenu
{
    internal class IDGenerator
    {
        private readonly Queue<int> recycledIds = new Queue<int>();
        private int currentId = (int)SysCommand.SC_RESTORE + 1; //0xF000;
        public int separatorPosition { get; set; } = -1;

        public int GetNewIdItem()
        {
            return this.recycledIds.Count > 0 ? this.recycledIds.Dequeue() : this.currentId++;
        }

        public void ReleaseIdItem(int id)
        {
            if (id >= ((int)SysCommand.SC_RESTORE + 1) && !this.recycledIds.Contains(id))
                this.recycledIds.Enqueue(id);
        }
    }
}