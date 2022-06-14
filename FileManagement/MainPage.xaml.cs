using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileManagement
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private StorageFile samplefile;

        private async void createfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = input.Text;
                StorageFolder sf = ApplicationData.Current.LocalFolder;
                this.samplefile = await sf.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                status.Text = "The file '" + samplefile.Name + "'was created.";
            }
            catch (FileNotFoundException)
            {
                status.Text = "First create a file";
            }
        }

        private async Task openfile()
        {
            string filename = input.Text;
            StorageFolder sf = ApplicationData.Current.LocalFolder;
            this.samplefile = await sf.GetFileAsync(filename);
            status.Text = "The file'" + samplefile.Name + "'was loaded.";
        }

        private async Task readfile()
        {
            using (IRandomAccessStream sessionra = await this.samplefile.OpenAsync(FileAccessMode.Read))
            {
                if (sessionra.Size > 0)
                {
                    byte[] array3 = new byte[sessionra.Size];
                    IBuffer output = await sessionra.ReadAsync(array3.AsBuffer(0, (int)sessionra.Size), (uint)sessionra.Size, InputStreamOptions.Partial);
                    string reread = Encoding.UTF8.GetString(output.ToArray(), 0, (int)output.Length);
                    status.Text = "The following text was read from'" + this.samplefile.Name + "'using a stream:" + Environment.NewLine + reread;
                }
                else
                {
                    status.Text = "File is empty";
                }
            }
        }

        private async void openandread_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.samplefile == null)
                {
                    await openfile();
                }
                if (this.samplefile == null)
                {
                    await readfile();
                }
                else
                {
                    status.Text = "Invalid handle! Please create the file first.";
                }
            }
            catch (FileNotFoundException)
            {
                status.Text = "File doesn't exist!";
            }
        }

        private async void appendandsave_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = this.samplefile;
            if (file != null)
            {
                string usercontent = input.Text;
                if (!string.IsNullOrEmpty(usercontent))
                {
                    using (IRandomAccessStream sessionra = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        Stream stream = sessionra.AsStreamForWrite();
                        if (stream.Length > 0)
                        {
                            stream.Seek(stream.Length, SeekOrigin.Begin);
                        }
                        byte[] array = Encoding.UTF8.GetBytes(usercontent);
                        stream.SetLength(stream.Length + array.Length);
                        await stream.WriteAsync(array, 0, array.Length);
                        await stream.FlushAsync();
                        await sessionra.FlushAsync();
                    }
                    await readfile();
                }
                else
                {
                    status.Text = "The textbox is empty, please write something and then click 'Save' again.";
                }
                resetscenariooutput();
            }
            else
            {
                status.Text = "File not open!";
            }
        }

        private void resetscenariooutput()
        {
            this.status.Text = "";
            this.input.Text = "";
        }

        private void notifyfilenotexist()
        {
            status.Text = "File does not exist!";
        }

        private async void delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.resetscenariooutput();
                StorageFile file = this.samplefile;

                if (file == null)
                {
                    await openfile();
                }
                if (file != null)
                {
                    string filename = file.Name;
                    await file.DeleteAsync();
                    this.samplefile = null;
                    status.Text = "The file'" + filename + "'was deleted.";
                }
                else
                {
                    status.Text = "The file does not exist!";
                }
            }
            catch (FileNotFoundException)
            {
                this.notifyfilenotexist();
            }
        }
    }
}
