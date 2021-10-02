using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.TreeShape.Models
{
    public class TreeNode
    {
        public void AddColumn(string input, string displayName, bool status = false)
        {
            if (Columns is null || Columns == default)
                Columns = new List<(string, string, bool)>();
            Columns.Add(new (input, displayName, status));
            
        }
 
        public void AddNode(string input, string displayName)
        {
            if (TreeNodes is null || TreeNodes == default)
                TreeNodes = new List<TreeNode>();
            TreeNodes.Add(new TreeNode() { TableName = new(input, displayName) });
          
        }

        
        public void Reset()
        {
            Columns?.Clear();
            TreeNodes?.Clear();
        }

        //ToDo - Add Lazy loading
        public List<TreeNode> TreeNodes { get; set; }
        public TreeNode Parent { get; set; } //ToDo Make it private
        public List<(string name,string displayName,bool status)> Columns { get; set; } //ToDo Make it private
        public (string name, string displayName) TableName { get; set; }
    }
}
