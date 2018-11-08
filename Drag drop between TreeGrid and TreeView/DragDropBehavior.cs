
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.Data.Extensions;

namespace Drag_drop_between_TreeGrid_and_TreeView
{
    /// <summary>
    /// DragDropBehavior model class
    /// </summary>
   public class DragDropBehavior:Behavior<MainWindow>
   {
        /// <summary>
        /// Initialize the OnAttached method
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        /// <summary>
        /// Wire the loaded event of this class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AssociatedObject.sfTreeGrid.RowDragDropController.DragStart += RowDragDropController_DragStart;
            this.AssociatedObject.sfTreeGrid.RowDragDropController.Drop += RowDragDropController_Drop;
            this.AssociatedObject.treeview.Drop += Treeview_Drop;
        }

        /// <summary>
        /// Describes the Drop event of the TreeView. It handled the drop items from SfTreeGrid to TreeViewAdv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Treeview_Drop(object sender, DragEventArgs e)
        {
            ObservableCollection<TreeNode> treeNodes = new ObservableCollection<TreeNode>();

            if (e.Data.GetDataPresent("Nodes"))
                treeNodes = e.Data.GetData("Nodes") as ObservableCollection<TreeNode>;

            EmployeeInfo item = new EmployeeInfo();

            if (treeNodes.Count == 0 || treeNodes == null)
                return;

            foreach (var node in treeNodes)
            {
                (AssociatedObject.sfTreeGrid.ItemsSource as ObservableCollection<EmployeeInfo>).Remove(node.Item as EmployeeInfo);
            }
        }

        /// <summary>
        /// Describes the Drop event of the RowDragDropController. It handled the drop items from TreeVideAdv to SfTreeGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowDragDropController_Drop(object sender, TreeGridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
            {
                ObservableCollection<object> item = e.Data.GetData(typeof(ObservableCollection<object>)) as ObservableCollection<object>;
                var record = item[0] as EmployeeInfo;
                var dropPosition = e.DropPosition.ToString();
                var newItem = new EmployeeInfo();

                var rowIndex = AssociatedObject.sfTreeGrid.ResolveToRowIndex(e.TargetNode.Item);
                int nodeIndex = (int)rowIndex;
                if (dropPosition != "None" && rowIndex != -1)
                {
                    if (AssociatedObject.sfTreeGrid.View is TreeGridSelfRelationalView)
                    {
                        var treeNode = AssociatedObject.sfTreeGrid.GetNodeAtRowIndex(rowIndex);

                        if (treeNode == null)
                            return;
                        var data = treeNode.Item;
                        AssociatedObject.sfTreeGrid.SelectionController.SuspendUpdates();
                        var itemIndex = -1;

                        TreeNode parentNode = null;

                        if (dropPosition == "DropBelow" || dropPosition == "DropAbove")
                        {
                            parentNode = treeNode.ParentNode;

                            if (parentNode == null)
                                newItem = new EmployeeInfo() { FirstName = record.FirstName, LastName = record.LastName, ID = record.ID, Salary = record.Salary, Title = record.Title, ReportsTo = -1 };
                            else
                            {
                                var parentkey = parentNode.Item as EmployeeInfo;
                                newItem = new EmployeeInfo() { FirstName = record.FirstName, LastName = record.LastName, ID = record.ID, Salary = record.Salary, Title = record.Title, ReportsTo = parentkey.ID };
                            }
                        }

                        else if (dropPosition == "DropAsChild")
                        {

                            if (!treeNode.IsExpanded)
                                AssociatedObject.sfTreeGrid.ExpandNode(treeNode);
                            parentNode = treeNode;
                            var parentkey = parentNode.Item as EmployeeInfo;
                            newItem = new EmployeeInfo() { FirstName = record.FirstName, LastName = record.LastName, ID = record.ID, Salary = record.Salary, Title = record.Title, ReportsTo = parentkey.ID };

                        }
                        IList sourceCollection = null;

                        if (dropPosition == "DropBelow" || dropPosition == "DropAbove")
                        {

                            if (treeNode.ParentNode != null)
                            {

                                var collection = AssociatedObject.sfTreeGrid.View.GetPropertyAccessProvider().GetValue(treeNode.ParentNode.Item, AssociatedObject.sfTreeGrid.ChildPropertyName) as IEnumerable;

                                sourceCollection = GetSourceListCollection(collection);
                            }

                            else
                            {
                                sourceCollection = GetSourceListCollection(AssociatedObject.sfTreeGrid.View.SourceCollection);
                            }
                            itemIndex = sourceCollection.IndexOf(data);

                            if (dropPosition == "DropBelow")
                            {
                                itemIndex += 1;
                            }
                        }

                        else if (dropPosition == "DropAsChild")
                        {
                            var collection = AssociatedObject.sfTreeGrid.View.GetPropertyAccessProvider().GetValue(data, AssociatedObject.sfTreeGrid.ChildPropertyName) as IEnumerable;

                            sourceCollection = GetSourceListCollection(collection);

                            if (sourceCollection == null)
                            {
                                var list = data.GetType().GetProperty(AssociatedObject.sfTreeGrid.ChildPropertyName).PropertyType.CreateNew() as IList;

                                if (list != null)
                                {
                                    AssociatedObject.sfTreeGrid.View.GetPropertyAccessProvider().SetValue(treeNode.Item, AssociatedObject.sfTreeGrid.ChildPropertyName, list);
                                    sourceCollection = list;
                                }
                            }
                            itemIndex = sourceCollection.Count;
                        }
                        sourceCollection.Insert(itemIndex, newItem);
                        AssociatedObject.sfTreeGrid.SelectionController.ResumeUpdates();
                        (AssociatedObject.sfTreeGrid.SelectionController as TreeGridRowSelectionController).RefreshSelection();
                        e.Handled = true;
                    }
                }
                (AssociatedObject.treeview.ItemsSource as ObservableCollection<EmployeeInfo>).Remove(record as EmployeeInfo);
            }
        }
        ObservableCollection<object> records = new ObservableCollection<object>();
        
        /// <summary>
        /// Describes the DragStart event of the RowDragDropController. It handled the drag start operaion of SfTreeGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowDragDropController_DragStart(object sender, TreeGridRowDragStartEventArgs e)
        {
            e.Handled = true;
            var dataObject = new DataObject();
            dataObject.SetData("SourceTreeGrid", this.AssociatedObject.sfTreeGrid);
            dataObject.SetData("Nodes", e.DraggingNodes);
            
            foreach (var node in e.DraggingNodes)
            {
                if (node.HasChildNodes)
                {
                    records.Add(node.Item as EmployeeInfo);
                    GetChildNodes(node);
                }
                else
                {
                    records.Add(node.Item as EmployeeInfo);
                }
            }

            dataObject.SetData(records);

            if(records!=null)
            DragDrop.DoDragDrop(this.AssociatedObject.sfTreeGrid, dataObject, DragDropEffects.Copy | DragDropEffects.Move);
            records.Clear();
        }

        /// <summary>
        /// Get the child nodes from parent node
        /// </summary>
        /// <param name="node"></param>
        private void GetChildNodes(TreeNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                records.Add(childNode.Item as EmployeeInfo);
                GetChildNodes(childNode);
            }
        }

        /// <summary>
        /// Get the source list collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private IList GetSourceListCollection(IEnumerable collection)
        {
            IList list = null;
            if (collection == null)
                collection = this.AssociatedObject.sfTreeGrid.View.SourceCollection;
            if ((collection as IList) != null)
            {
                list = collection as IList;
            }
            return list;
        }
    }
}
