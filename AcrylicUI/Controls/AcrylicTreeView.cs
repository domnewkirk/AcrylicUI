﻿using AcrylicUI.Collections;
using AcrylicUI.Forms;
using AcrylicUI.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AcrylicUI.Controls
{
    public class AcrylicTreeView : AcrylicScrollView
    {
        #region Event Region

        public event EventHandler SelectedNodesChanged;
        public event EventHandler AfterNodeExpand;
        public event EventHandler AfterNodeCollapse;

        #endregion

        #region Field Region

        private bool _disposed;

        private readonly int _expandAreaSize = 16;
        private readonly int _iconSize = 16;
        private readonly int _plusIconSize = 12;
        private int _itemHeight = 20;
        private int _indent = 20;

        private ObservableList<AcrylicTreeNode> _nodes;
        private ObservableCollection<AcrylicTreeNode> _selectedNodes;

        private AcrylicTreeNode _anchoredNodeStart;
        private AcrylicTreeNode _anchoredNodeEnd;


        private IconFactory _iconFactory;

        private Image _nodeClosed;
        private Image _nodeClosedHover;
        private Image _nodeClosedHoverSelected;
        private Image _nodeOpen;
        private Image _nodeOpenHover;
        private Image _nodeOpenHoverSelected;

        private AcrylicTreeNode _provisionalNode;
        private AcrylicTreeNode _dropNode;
        private bool _provisionalDragging;
        private List<AcrylicTreeNode> _dragNodes;
        private Point _dragPos;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableList<AcrylicTreeNode> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != null)
                {
                    _nodes.ItemsAdded -= Nodes_ItemsAdded;
                    _nodes.ItemsRemoved -= Nodes_ItemsRemoved;

                    foreach (var node in _nodes)
                        UnhookNodeEvents(node);
                }

                _nodes = value;

                _nodes.ItemsAdded += Nodes_ItemsAdded;
                _nodes.ItemsRemoved += Nodes_ItemsRemoved;

                foreach (var node in _nodes)
                    HookNodeEvents(node);

                UpdateNodes();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<AcrylicTreeNode> SelectedNodes
        {
            get { return _selectedNodes; }
        }

        [Category("Appearance")]
        [Description("Determines the height of tree nodes.")]
        [DefaultValue(20)]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                _itemHeight = value;
                MaxDragChange = _itemHeight;
                UpdateNodes();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of horizontal space given by parent node.")]
        [DefaultValue(20)]
        public int Indent
        {
            get { return _indent; }
            set
            {
                _indent = value;
                UpdateNodes();
            }
        }

        [Category("Behavior")]
        [Description("Determines whether multiple tree nodes can be selected at once.")]
        [DefaultValue(false)]
        public bool MultiSelect { get; set; }

        [Category("Behavior")]
        [Description("Determines whether nodes can be moved within this tree view.")]
        [DefaultValue(false)]
        public bool AllowMoveNodes { get; set; }

        [Category("Appearance")]
        [Description("Determines whether icons are rendered with the tree nodes.")]
        [DefaultValue(false)]
        public bool ShowIcons { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int VisibleNodeCount { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IComparer<AcrylicTreeNode> TreeViewNodeSorter { get; set; }

        #endregion

        #region Constructor Region

        public AcrylicTreeView()
        {
            this.BackColor = Colors.GreyBackground;
            this.Dock = DockStyle.Fill;

            Nodes = new ObservableList<AcrylicTreeNode>();
            _selectedNodes = new ObservableCollection<AcrylicTreeNode>();
            _selectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

            MaxDragChange = _itemHeight;

            LoadIcons();
            UpdateScale();
        }

        #endregion

        #region Dispose Region

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DisposeIcons();

                if (SelectedNodesChanged != null)
                    SelectedNodesChanged = null;

                if (AfterNodeExpand != null)
                    AfterNodeExpand = null;

                if (AfterNodeCollapse != null)
                    AfterNodeExpand = null;

                if (_nodes != null)
                    _nodes.Dispose();

                if (_selectedNodes != null)
                    _selectedNodes.CollectionChanged -= SelectedNodes_CollectionChanged;

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Event Handler Region

        private void Nodes_ItemsAdded(object sender, ObservableListModified<AcrylicTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                node.ParentTree = this;
                node.IsRoot = true;

                HookNodeEvents(node);
            }

            if (TreeViewNodeSorter != null)
                Nodes.Sort(TreeViewNodeSorter);

            UpdateNodes();
        }

        private void Nodes_ItemsRemoved(object sender, ObservableListModified<AcrylicTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                node.ParentTree = this;
                node.IsRoot = true;

                HookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void ChildNodes_ItemsAdded(object sender, ObservableListModified<AcrylicTreeNode> e)
        {
            foreach (var node in e.Items)
                HookNodeEvents(node);

            UpdateNodes();
        }

        private void ChildNodes_ItemsRemoved(object sender, ObservableListModified<AcrylicTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                if (SelectedNodes.Contains(node))
                    SelectedNodes.Remove(node);

                UnhookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedNodesChanged != null)
                SelectedNodesChanged(this, null);
        }

        private void Nodes_TextChanged(object sender, EventArgs e)
        {
            UpdateNodes();
        }

        private void Nodes_NodeExpanded(object sender, EventArgs e)
        {
            UpdateNodes();

            if (AfterNodeExpand != null)
                AfterNodeExpand(this, null);
        }

        private void Nodes_NodeCollapsed(object sender, EventArgs e)
        {
            UpdateNodes();

            if (AfterNodeCollapse != null)
                AfterNodeCollapse(this, null);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_provisionalDragging)
            {
                if (OffsetMousePosition != _dragPos)
                {
                    StartDrag();
                    HandleDrag();
                    return;
                }
            }

            if (IsDragging)
            {
                if (_dropNode != null)
                {
                    var rect = GetNodeFullRowArea(_dropNode);
                    if (!rect.Contains(OffsetMousePosition))
                    {
                        _dropNode = null;
                        Invalidate();
                    }
                }
            }

            CheckHover();

            if (IsDragging)
            {
                HandleDrag();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            CheckHover();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                foreach (var node in Nodes)
                    CheckNodeClick(node, OffsetMousePosition, e.Button);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsDragging)
            {
                HandleDrop();
            }

            if (_provisionalDragging && MultiSelect)
            {

                if (_provisionalNode != null)
                {
                    var pos = _dragPos;
                    if (OffsetMousePosition == pos)
                        SelectNode(_provisionalNode);
                }

                _provisionalDragging = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
                return;

            if (e.Button == MouseButtons.Left)
            {
                foreach (var node in Nodes)
                    CheckNodeDoubleClick(node, OffsetMousePosition);
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (var node in Nodes)
                NodeMouseLeave(node);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsDragging)
                return;

            if (Nodes.Count == 0)
                return;

            if (e.KeyCode != Keys.Down && e.KeyCode != Keys.Up && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                return;

            if (_anchoredNodeEnd == null)
            {
                if (Nodes.Count > 0)
                    SelectNode(Nodes[0]);
                return;
            }

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (MultiSelect && ModifierKeys == Keys.Shift)
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        if (_anchoredNodeEnd.PrevVisibleNode != null)
                        {
                            SelectAnchoredRange(_anchoredNodeEnd.PrevVisibleNode);
                            EnsureVisible();
                        }
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        if (_anchoredNodeEnd.NextVisibleNode != null)
                        {
                            SelectAnchoredRange(_anchoredNodeEnd.NextVisibleNode);
                            EnsureVisible();
                        }
                    }
                }
                else
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        if (_anchoredNodeEnd.PrevVisibleNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.PrevVisibleNode);
                            EnsureVisible();
                        }
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        if (_anchoredNodeEnd.NextVisibleNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.NextVisibleNode);
                            EnsureVisible();
                        }
                    }
                }
            }

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                if (e.KeyCode == Keys.Left)
                {
                    if (_anchoredNodeEnd.Expanded && _anchoredNodeEnd.Nodes.Count > 0)
                    {
                        _anchoredNodeEnd.Expanded = false;
                    }
                    else
                    {
                        if (_anchoredNodeEnd.ParentNode != null)
                        {
                            SelectNode(_anchoredNodeEnd.ParentNode);
                            EnsureVisible();
                        }
                    }
                }
                else if (e.KeyCode == Keys.Right)
                {
                    if (!_anchoredNodeEnd.Expanded)
                    {
                        _anchoredNodeEnd.Expanded = true;
                    }
                    else
                    {
                        if (_anchoredNodeEnd.Nodes.Count > 0)
                        {
                            SelectNode(_anchoredNodeEnd.Nodes[0]);
                            EnsureVisible();
                        }
                    }
                }
            }
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            if (!IsDragging)
            {
                StopDrag();
                return;
            }

            if (MouseButtons != MouseButtons.Left)
            {
                StopDrag();
                return;
            }

            var pos = PointToClient(MousePosition);

            if (_vScrollBar.Visible)
            {
                // Scroll up
                if (pos.Y < ClientRectangle.Top)
                {
                    var difference = (pos.Y - ClientRectangle.Top) * -1;

                    if (difference > Scale(ItemHeight))
                        difference = Scale(ItemHeight);

                    _vScrollBar.Value = _vScrollBar.Value - difference;
                }

                // Scroll down
                if (pos.Y > ClientRectangle.Bottom)
                {
                    var difference = pos.Y - ClientRectangle.Bottom;

                    if (difference > Scale(ItemHeight))
                        difference = Scale(ItemHeight);

                    _vScrollBar.Value = _vScrollBar.Value + difference;
                }
            }

            if (_hScrollBar.Visible)
            {
                // Scroll left
                if (pos.X < ClientRectangle.Left)
                {
                    var difference = (pos.X - ClientRectangle.Left) * -1;

                    if (difference > Scale(ItemHeight))
                        difference = Scale(ItemHeight);

                    _hScrollBar.Value = _hScrollBar.Value - difference;
                }

                // Scroll right
                if (pos.X > ClientRectangle.Right)
                {
                    var difference = pos.X - ClientRectangle.Right;

                    if (difference > Scale(ItemHeight))
                        difference = Scale(ItemHeight);

                    _hScrollBar.Value = _hScrollBar.Value + difference;
                }
            }
        }

        #endregion

        #region Method Region

        private void HookNodeEvents(AcrylicTreeNode node)
        {
            node.Nodes.ItemsAdded += ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved += ChildNodes_ItemsRemoved;

            node.TextChanged += Nodes_TextChanged;
            node.NodeExpanded += Nodes_NodeExpanded;
            node.NodeCollapsed += Nodes_NodeCollapsed;

            foreach (var childNode in node.Nodes)
                HookNodeEvents(childNode);
        }

        private void UnhookNodeEvents(AcrylicTreeNode node)
        {
            node.Nodes.ItemsAdded -= ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved -= ChildNodes_ItemsRemoved;

            node.TextChanged -= Nodes_TextChanged;
            node.NodeExpanded -= Nodes_NodeExpanded;
            node.NodeCollapsed -= Nodes_NodeCollapsed;

            foreach (var childNode in node.Nodes)
                UnhookNodeEvents(childNode);
        }

        private void UpdateNodes()
        {
            if (IsDragging)
                return;

            ContentSize = new Size(0, 0);

            if (Nodes.Count == 0)
                return;

            var yOffset = 0;
            var isOdd = false;
            var index = 0;
            AcrylicTreeNode prevNode = null;

            for (var i = 0; i <= Nodes.Count - 1; i++)
            {
                var node = Nodes[i];
                UpdateNode(node, ref prevNode, 0, ref yOffset, ref isOdd, ref index);
            }

            ContentSize = new Size(ContentSize.Width, yOffset);

            VisibleNodeCount = index;

            Invalidate();
        }

        private void UpdateNode(AcrylicTreeNode node, ref AcrylicTreeNode prevNode, int indent, ref int yOffset,
                                ref bool isOdd, ref int index)
        {
            UpdateNodeBounds(node, yOffset, indent);

            yOffset += Scale(ItemHeight);

            node.Odd = isOdd;
            isOdd = !isOdd;

            node.VisibleIndex = index;
            index++;

            node.PrevVisibleNode = prevNode;

            if (prevNode != null)
                prevNode.NextVisibleNode = node;

            prevNode = node;

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    UpdateNode(childNode, ref prevNode, indent + Scale(Indent), ref yOffset, ref isOdd, ref index);
            }
        }

        private void UpdateNodeBounds(AcrylicTreeNode node, int yOffset, int indent)
        {
            var expandTop = yOffset + (Scale(ItemHeight) / 2) - (Scale(_expandAreaSize) / 2);
            node.ExpandArea = new Rectangle(indent + Scale(3), expandTop, Scale(_expandAreaSize), Scale(_expandAreaSize));

            var iconTop = yOffset + (Scale(ItemHeight) / 2) - (Scale(_iconSize) / 2);

            if (ShowIcons)
                node.IconArea = new Rectangle(node.ExpandArea.Right + Scale(2), iconTop, Scale(_iconSize), Scale(_iconSize));
            else
                node.IconArea = new Rectangle(node.ExpandArea.Right, iconTop, 0, 0);

            using (var g = CreateGraphics())
            {
                var textSize = (int)(g.MeasureString(node.Text, Font).Width);
                node.TextArea = new Rectangle(node.IconArea.Right + Scale(2), yOffset, textSize + Scale(1), Scale(ItemHeight));
            }

            node.FullArea = new Rectangle(indent, yOffset, (node.TextArea.Right - indent), Scale(ItemHeight));

            if (ContentSize.Width < node.TextArea.Right + Scale(2))
                ContentSize = new Size(node.TextArea.Right + Scale(2), ContentSize.Height);
        }

        private void LoadIcons()
        {
            DisposeIcons();


            var dpiScale = IconFactory.GetDpiScale(this.Handle);
            _iconFactory = new IconFactory(IconFactory.GetDpiScale(Handle));

            _nodeClosed = _iconFactory.BitmapFromSvg(TreeViewIcons.GlyphRight_16x, _plusIconSize, _plusIconSize);
            _nodeClosedHover = _iconFactory.BlueBitmapFromSvg(TreeViewIcons.GlyphRight_16x, _plusIconSize, _plusIconSize);
            _nodeClosedHoverSelected = _iconFactory.NamedColorBitmapFromSvg(TreeViewIcons.GlyphRight_16x, Colors.GreyShadow, Color.White, _plusIconSize, _plusIconSize);

            _nodeOpen = _iconFactory.NamedColorBitmapFromSvg(TreeViewIcons.ScrollbarArrowsDownRight_16x_svg, Colors.GreyShadow, Color.White, _plusIconSize, _plusIconSize);
            _nodeOpenHover = _iconFactory.BlueBitmapFromSvg(TreeViewIcons.ScrollbarArrowsDownRight_16x_svg, _plusIconSize, _plusIconSize);
            _nodeOpenHoverSelected = _iconFactory.NamedColorBitmapFromSvg(TreeViewIcons.ScrollbarArrowsDownRight_16x_svg, Colors.GreyShadow, Colors.BlueSelection, _plusIconSize, _plusIconSize);
        }

        private void DisposeIcons()
        {
            if (_nodeClosed != null)
                _nodeClosed.Dispose();

            if (_nodeClosedHover != null)
                _nodeClosedHover.Dispose();

            if (_nodeClosedHoverSelected != null)
                _nodeClosedHoverSelected.Dispose();

            if (_nodeOpen != null)
                _nodeOpen.Dispose();

            if (_nodeOpenHover != null)
                _nodeOpenHover.Dispose();

            if (_nodeOpenHoverSelected != null)
                _nodeOpenHoverSelected.Dispose();
        }

        private void CheckHover()
        {
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                if (IsDragging)
                {
                    if (_dropNode != null)
                    {
                        _dropNode = null;
                        Invalidate();
                    }
                }

                return;
            }

            foreach (var node in Nodes)
                CheckNodeHover(node, OffsetMousePosition);
        }

        private void NodeMouseLeave(AcrylicTreeNode node)
        {
            node.ExpandAreaHot = false;

            foreach (var childNode in node.Nodes)
                NodeMouseLeave(childNode);

            Invalidate();
        }

        private void CheckNodeHover(AcrylicTreeNode node, Point location)
        {
            if (IsDragging)
            {
                var rect = GetNodeFullRowArea(node);
                if (rect.Contains(OffsetMousePosition))
                {
                    var newDropNode = _dragNodes.Contains(node) ? null : node;

                    if (_dropNode != newDropNode)
                    {
                        _dropNode = newDropNode;
                        Invalidate();
                    }
                }
            }
            else
            {
                var hot = node.ExpandArea.Contains(location);
                if (node.ExpandAreaHot != hot)
                {
                    node.ExpandAreaHot = hot;
                    Invalidate();
                }
            }

            foreach (var childNode in node.Nodes)
                CheckNodeHover(childNode, location);
        }

        private void CheckNodeClick(AcrylicTreeNode node, Point location, MouseButtons button)
        {
            var rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (node.ExpandArea.Contains(location))
                {
                    if (button == MouseButtons.Left)
                        node.Expanded = !node.Expanded;
                }
                else
                {
                    if (button == MouseButtons.Left)
                    {
                        if (MultiSelect && ModifierKeys == Keys.Shift)
                        {
                            SelectAnchoredRange(node);
                        }
                        else if (MultiSelect && ModifierKeys == Keys.Control)
                        {
                            ToggleNode(node);
                        }
                        else
                        {
                            if (!SelectedNodes.Contains(node))
                                SelectNode(node);

                            _dragPos = OffsetMousePosition;
                            _provisionalDragging = true;
                            _provisionalNode = node;
                        }

                        return;
                    }
                    else if (button == MouseButtons.Right)
                    {
                        if (MultiSelect && ModifierKeys == Keys.Shift)
                            return;

                        if (MultiSelect && ModifierKeys == Keys.Control)
                            return;

                        if (!SelectedNodes.Contains(node))
                            SelectNode(node);

                        return;
                    }
                }
            }

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    CheckNodeClick(childNode, location, button);
            }
        }

        private void CheckNodeDoubleClick(AcrylicTreeNode node, Point location)
        {
            var rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (!node.ExpandArea.Contains(location))
                    node.Expanded = !node.Expanded;

                return;
            }

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    CheckNodeDoubleClick(childNode, location);
            }
        }

        public void SelectNode(AcrylicTreeNode node)
        {
            _selectedNodes.Clear();
            _selectedNodes.Add(node);

            _anchoredNodeStart = node;
            _anchoredNodeEnd = node;

            Invalidate();
        }

        public void SelectNodes(AcrylicTreeNode startNode, AcrylicTreeNode endNode)
        {
            var nodes = new List<AcrylicTreeNode>();

            if (startNode == endNode)
                nodes.Add(startNode);

            if (startNode.VisibleIndex < endNode.VisibleIndex)
            {
                var node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.NextVisibleNode;
                    nodes.Add(node);
                }
            }
            else if (startNode.VisibleIndex > endNode.VisibleIndex)
            {
                var node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.PrevVisibleNode;
                    nodes.Add(node);
                }
            }

            SelectNodes(nodes, false);
        }

        public void SelectNodes(List<AcrylicTreeNode> nodes, bool updateAnchors = true)
        {
            _selectedNodes.Clear();

            foreach (var node in nodes)
                _selectedNodes.Add(node);

            if (updateAnchors && _selectedNodes.Count > 0)
            {
                _anchoredNodeStart = _selectedNodes[_selectedNodes.Count - 1];
                _anchoredNodeEnd = _selectedNodes[_selectedNodes.Count - 1];
            }

            Invalidate();
        }

        private void SelectAnchoredRange(AcrylicTreeNode node)
        {
            _anchoredNodeEnd = node;
            SelectNodes(_anchoredNodeStart, _anchoredNodeEnd);
        }

        public void ToggleNode(AcrylicTreeNode node)
        {
            if (_selectedNodes.Contains(node))
            {
                _selectedNodes.Remove(node);

                // If we just removed both the anchor start AND end then reset them
                if (_anchoredNodeStart == node && _anchoredNodeEnd == node)
                {
                    if (_selectedNodes.Count > 0)
                    {
                        _anchoredNodeStart = _selectedNodes[0];
                        _anchoredNodeEnd = _selectedNodes[0];
                    }
                    else
                    {
                        _anchoredNodeStart = null;
                        _anchoredNodeEnd = null;
                    }
                }

                // If we just removed the anchor start then update it accordingly
                if (_anchoredNodeStart == node)
                {
                    if (_anchoredNodeEnd.VisibleIndex < node.VisibleIndex)
                        _anchoredNodeStart = node.PrevVisibleNode;
                    else if (_anchoredNodeEnd.VisibleIndex > node.VisibleIndex)
                        _anchoredNodeStart = node.NextVisibleNode;
                    else
                        _anchoredNodeStart = _anchoredNodeEnd;
                }

                // If we just removed the anchor end then update it accordingly
                if (_anchoredNodeEnd == node)
                {
                    if (_anchoredNodeStart.VisibleIndex < node.VisibleIndex)
                        _anchoredNodeEnd = node.PrevVisibleNode;
                    else if (_anchoredNodeStart.VisibleIndex > node.VisibleIndex)
                        _anchoredNodeEnd = node.NextVisibleNode;
                    else
                        _anchoredNodeEnd = _anchoredNodeStart;
                }
            }
            else
            {
                _selectedNodes.Add(node);

                _anchoredNodeStart = node;
                _anchoredNodeEnd = node;
            }

            Invalidate();
        }

        public Rectangle GetNodeFullRowArea(AcrylicTreeNode node)
        {
            if (node.ParentNode != null && !node.ParentNode.Expanded)
                return new Rectangle(-1, -1, -1, -1);

            var width = Math.Max(ContentSize.Width, Viewport.Width);
            var rect = new Rectangle(0, node.FullArea.Top, width, Scale(ItemHeight));
            return rect;
        }

        public void EnsureVisible()
        {
            if (SelectedNodes.Count == 0)
                return;

            foreach (var node in SelectedNodes)
                node.EnsureVisible();

            var itemTop = -1;

            if (!MultiSelect)
                itemTop = SelectedNodes[0].FullArea.Top;
            else
                itemTop = _anchoredNodeEnd.FullArea.Top;

            var itemBottom = itemTop + Scale(ItemHeight);

            if (itemTop < Viewport.Top)
                VScrollTo(itemTop);

            if (itemBottom > Viewport.Bottom)
                VScrollTo((itemBottom - Viewport.Height));
        }

        public void Sort()
        {
            if (TreeViewNodeSorter == null)
                return;

            Nodes.Sort(TreeViewNodeSorter);

            foreach (var node in Nodes)
                SortChildNodes(node);
        }

        private void SortChildNodes(AcrylicTreeNode node)
        {
            node.Nodes.Sort(TreeViewNodeSorter);

            foreach (var childNode in node.Nodes)
                SortChildNodes(childNode);
        }

        public AcrylicTreeNode FindNode(string path)
        {
            foreach (var node in Nodes)
            {
                var compNode = FindNode(node, path);
                if (compNode != null)
                    return compNode;
            }

            return null;
        }

        private AcrylicTreeNode FindNode(AcrylicTreeNode parentNode, string path, bool recursive = true)
        {
            if (parentNode.FullPath == path)
                return parentNode;

            foreach (var node in parentNode.Nodes)
            {
                if (node.FullPath == path)
                    return node;

                if (recursive)
                {
                    var compNode = FindNode(node, path);
                    if (compNode != null)
                        return compNode;
                }
            }

            return null;
        }

        #endregion

        #region Drag & Drop Region

        protected override void StartDrag()
        {
            if (!AllowMoveNodes)
            {
                _provisionalDragging = false;
                return;
            }

            // Create initial list of nodes to drag
            _dragNodes = new List<AcrylicTreeNode>();
            foreach (var node in SelectedNodes)
                _dragNodes.Add(node);

            // Clear out any nodes with a parent that is being dragged
            foreach (var node in _dragNodes.ToList())
            {
                if (node.ParentNode == null)
                    continue;

                if (_dragNodes.Contains(node.ParentNode))
                    _dragNodes.Remove(node);
            }

            _provisionalDragging = false;

            Cursor = Cursors.SizeAll;

            base.StartDrag();
        }

        private void HandleDrag()
        {
            if (!AllowMoveNodes)
                return;

            var dropNode = _dropNode;

            if (dropNode == null)
            {
                if (Cursor != Cursors.No)
                    Cursor = Cursors.No;

                return;
            }

            if (ForceDropToParent(dropNode))
                dropNode = dropNode.ParentNode;

            if (!CanMoveNodes(_dragNodes, dropNode))
            {
                if (Cursor != Cursors.No)
                    Cursor = Cursors.No;

                return;
            }

            if (Cursor != Cursors.SizeAll)
                Cursor = Cursors.SizeAll;
        }

        private void HandleDrop()
        {
            if (!AllowMoveNodes)
                return;

            var dropNode = _dropNode;

            if (dropNode == null)
            {
                StopDrag();
                return;
            }

            if (ForceDropToParent(dropNode))
                dropNode = dropNode.ParentNode;

            if (CanMoveNodes(_dragNodes, dropNode, true))
            {
                var cachedSelectedNodes = SelectedNodes.ToList();

                MoveNodes(_dragNodes, dropNode);

                foreach (var node in _dragNodes)
                {
                    if (node.ParentNode == null)
                        Nodes.Remove(node);
                    else
                        node.ParentNode.Nodes.Remove(node);

                    dropNode.Nodes.Add(node);
                }

                if (TreeViewNodeSorter != null)
                    dropNode.Nodes.Sort(TreeViewNodeSorter);

                dropNode.Expanded = true;

                NodesMoved(_dragNodes);

                foreach (var node in cachedSelectedNodes)
                    _selectedNodes.Add(node);
            }

            StopDrag();
            UpdateNodes();
        }

        protected override void StopDrag()
        {
            _dragNodes = null;
            _dropNode = null;

            Cursor = Cursors.Default;

            Invalidate();

            base.StopDrag();
        }

        protected virtual bool ForceDropToParent(AcrylicTreeNode node)
        {
            return false;
        }

        protected virtual bool CanMoveNodes(List<AcrylicTreeNode> dragNodes, AcrylicTreeNode dropNode, bool isMoving = false)
        {
            if (dropNode == null)
                return false;

            foreach (var node in dragNodes)
            {
                if (node == dropNode)
                {
                    if (isMoving)
                        AcrylicMessageBox.ShowError($"Cannot move {node.Text}. The destination folder is the same as the source folder.", Application.ProductName);

                    return false;
                }

                if (node.ParentNode != null && node.ParentNode == dropNode)
                {
                    if (isMoving)
                        AcrylicMessageBox.ShowError($"Cannot move {node.Text}. The destination folder is the same as the source folder.", Application.ProductName);

                    return false;
                }

                var parentNode = dropNode.ParentNode;
                while (parentNode != null)
                {
                    if (node == parentNode)
                    {
                        if (isMoving)
                            AcrylicMessageBox.ShowError($"Cannot move {node.Text}. The destination folder is a subfolder of the source folder.", Application.ProductName);

                        return false;
                    }

                    parentNode = parentNode.ParentNode;
                }
            }

            return true;
        }

        protected virtual void MoveNodes(List<AcrylicTreeNode> dragNodes, AcrylicTreeNode dropNode)
        { }

        protected virtual void NodesMoved(List<AcrylicTreeNode> nodesMoved)
        { }

        #endregion

        #region Paint Region

        protected override void PaintContent(Graphics g)
        {
            foreach (var node in Nodes)
            {
                DrawNode(node, g);
            }
        }

        private void DrawNode(AcrylicTreeNode node, Graphics g)
        {
            var rect = GetNodeFullRowArea(node);

            // 1. Draw background
            var bgColor = node.Odd ? Colors.HeaderBackground : Colors.GreyBackground;

            if (SelectedNodes.Count > 0 && SelectedNodes.Contains(node))
                bgColor = Focused ? Colors.BlueSelection : Colors.GreySelection;

            if (IsDragging && _dropNode == node)
                bgColor = Focused ? Colors.BlueSelection : Colors.GreySelection;

            using (var b = new SolidBrush(bgColor))
            {
                g.FillRectangle(b, rect);
            }

            // 2. Draw plus/minus icon
            if (node.Nodes.Count > 0)
            {
                var pos = new Point(node.ExpandArea.Location.X + Scale(2), node.ExpandArea.Location.Y + Scale(2));

                var icon = _nodeOpen;

                if (node.Expanded && !node.ExpandAreaHot)
                    icon = _nodeOpen;
                else if (node.Expanded && node.ExpandAreaHot && !SelectedNodes.Contains(node))
                    icon = _nodeOpenHover;
                else if (node.Expanded && node.ExpandAreaHot && SelectedNodes.Contains(node))
                    icon = _nodeOpenHoverSelected;
                else if (!node.Expanded && !node.ExpandAreaHot)
                    icon = _nodeClosed;
                else if (!node.Expanded && node.ExpandAreaHot && !SelectedNodes.Contains(node))
                    icon = _nodeClosedHover;
                else if (!node.Expanded && node.ExpandAreaHot && SelectedNodes.Contains(node))
                    icon = _nodeClosedHoverSelected;

                g.DrawImage(icon, pos);
            }

            // 3. Draw icon
            if (ShowIcons && node.Icon != null)
            {
                node.UpdateScale(_dpiScale);
                if (node.Expanded && node.ExpandedIcon != null)
                    g.DrawImageUnscaled(node.ExpandedIcon, node.IconArea.Location);
                else
                    g.DrawImageUnscaled(node.Icon, node.IconArea.Location);
            }

            // 4. Draw text
            using (var b = new SolidBrush(Colors.LightText))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(node.Text, Font, b, node.TextArea, stringFormat);
            }

            // 5. Draw child nodes
            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    DrawNode(childNode, g);
            }
        }

        #endregion


        #region Dpi Scale

        private const float DEFAULT_DPI = 96f;
        private float _dpiScale = 1;

        // call at init too
        //private void UpdateScale()
        //{
        //    var form = FindForm();
        //    if (form is null)
        //    {

        //    }
        //    var handle = form?.Handle ?? this.Handle;

        //    var newDpiScale = (float)Drawing.GetDpi(handle) / (float)DEFAULT_DPI;
        //    if (newDpiScale != _dpiScale)
        //    {
        //        _dpiScale = newDpiScale;

        //        // TODO
        //        // update Icons
        //    }
        //}
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScale();
            UpdateNodes();
            LoadIcons();
        }
        private int Scale(int pixel)
        {
            return (int)(pixel * _dpiScale);
        }

        #endregion

    }
}
