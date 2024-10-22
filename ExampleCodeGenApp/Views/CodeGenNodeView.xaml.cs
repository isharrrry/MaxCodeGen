﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExampleCodeGenApp.ViewModels;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Views;
using ReactiveUI;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace ExampleCodeGenApp.Views
{
    public partial class CodeGenNodeView : IViewFor<CodeGenNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodeGenNodeViewModel), typeof(CodeGenNodeView), new PropertyMetadata(null));

        public CodeGenNodeViewModel ViewModel
        {
            get => (CodeGenNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CodeGenNodeViewModel)value;
        }
        #endregion

        public CodeGenNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => NodeView.ViewModel = null).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.NodeType, v => v.NodeView.Background, ConvertNodeTypeToBrush).DisposeWith(d);
            });
            DataContextChanged +=CodeGenNodeView_DataContextChanged;
            CodeGenNodeView_DataContextChanged(null, new DependencyPropertyChangedEventArgs());
        }

        private void CodeGenNodeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is BaseCodeGenNodeViewModel BaseCodeVm)
            {
                lbSetting.Visibility = Visibility.Visible;

            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PropertyGridShow((DataContext as BaseCodeGenNodeViewModel)?.ParamPropertyList, DataContext.GetType().Name.ToString());
        }

        public static void PropertyGridShow(object SelectedObject, string title)
        {
            PropertyGrid PG = new PropertyGrid();
            PG.SelectedObject = SelectedObject;
            PG.SelectedObjectTypeName = title;
            var win = new Window()
            {
                Title = "设置模块属性",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                //SizeToContent="WidthAndHeight"
                Width = 400,
                Height = 400,
                Content = PG,
            };
            win.Show();
        }

        public static  Brush ConvertNodeTypeToBrush(NodeType type)
        {
            switch (type)
            {
                case NodeType.EventNode: return new SolidColorBrush(Color.FromRgb(0x9b, 0x00, 0x00));
                case NodeType.FlowControl: return new SolidColorBrush(Color.FromRgb(0x49, 0x49, 0x49));
                case NodeType.Function: return new SolidColorBrush(Color.FromRgb(0x00, 0x39, 0xcb));
                case NodeType.Literal: return new SolidColorBrush(Color.FromRgb(0x00, 0x60, 0x0f));
                case NodeType.Group: return new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2));
                default: throw new Exception("Unsupported node type");
            }
        }
    }
}
