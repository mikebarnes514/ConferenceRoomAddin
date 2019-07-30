using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MJ.Scheduling
{
    class HeaderPanel : ItemsControl
    {
        #region Constructors
        static HeaderPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderPanel), new FrameworkPropertyMetadata(typeof(HeaderPanel)));
        }
        #endregion
    }
}
