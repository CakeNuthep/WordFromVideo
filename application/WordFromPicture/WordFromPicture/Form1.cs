using Accord.Video.FFMPEG;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordFromPicture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox_pathFile.Text = openFileDialog.FileName;
                    VideoFileReader reader = new VideoFileReader();
                    reader.Open(textBox_pathFile.Text);
                    pictureBox_input.Image = resizeImage(reader.ReadVideoFrame(),new Size(380,100));
                    reader.Close();
                }
            }
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            VideoFileReader reader = new VideoFileReader();
            reader.Open(textBox_pathFile.Text);
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(@"C:\Users\Cake\Desktop\test.docx", DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                ParagraphProperties bodyProperty = new ParagraphProperties();
                Justification justification1 = new Justification() { Val = JustificationValues.Center };
                bodyProperty.Append(justification1);
                Paragraph para = body.AppendChild(new Paragraph(bodyProperty));

                int iterator = (int)(reader.FrameRate / 3);

                for (int i = 0; i < 2000; i += iterator)
                {
                    Bitmap videoFrame = reader.ReadVideoFrame(i);
                    Bitmap image = (Bitmap)resizeImage(videoFrame, new Size(380, 100));
                    string str = textBox_word.Text;
                    int indexLetter = 0;
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {

                            System.Drawing.Color color = image.GetPixel(x, y);
                            DocumentFormat.OpenXml.Wordprocessing.Color wordColor = new DocumentFormat.OpenXml.Wordprocessing.Color() { Val = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") };
                            FontSize fontSize = new FontSize() { Val = "12" };

                            RunProperties runPro = new RunProperties();
                            runPro.Append(wordColor);
                            runPro.Append(fontSize);
                            
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(runPro);
                            char letter = str[indexLetter];

                            run.AppendChild(new Text(letter.ToString()));

                            if (str.Length > 0)
                            {
                                indexLetter = (indexLetter + 1) % str.Length;
                            }
                        }
                        Run runNewLine = para.AppendChild(new Run());
                        runNewLine.AppendChild(new Break());
                    }

                    Run runBreakPage = para.AppendChild(new Run());
                    runBreakPage.AppendChild(new Break() { Type=BreakValues.Page});

                    videoFrame.Dispose();
                    image.Dispose();
                }
            }
            
            reader.Close();
            MessageBox.Show("finish");

        }
    }
}
