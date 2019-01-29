using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
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

namespace CaptureToSharedMemory
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MemoryMappedFile share_mem;
        private MemoryMappedViewAccessor accessor;
        private byte counter = 1;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            share_mem = MemoryMappedFile.CreateOrOpen("SharedMemory", 800 * 450 * 4 + 5);
            accessor = share_mem.CreateViewAccessor();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var visual = Root;
            var bounds = VisualTreeHelper.GetDescendantBounds(visual);
            var bitmap = new RenderTargetBitmap(
                (int)bounds.Width,
                (int)bounds.Height,
                96.0,
                96.0,
                PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(visual);
                dc.DrawRectangle(vb, null, bounds);
            }

            bitmap.Render(dv);
            bitmap.Freeze();

            var buffer = new byte[800 * 450 * 4];
            bitmap.CopyPixels(buffer, 800 * 4, 0);

            // write counter
            accessor.Write(0, counter++);

            // write buffer size
            accessor.Write(1, buffer.Length);

            // write buffer
            accessor.WriteArray(5, buffer, 0, buffer.Length);
        }
    }
}
