using GRL.Utils;
using GRL.Utils.C2VEnums.DPEnum.Enums;
using GRL.Utils.Logger.WriteLog;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace PixelimageGeneration
{
    internal class Program
    {
        public int Counter = 0;
        public Dictionary<string, int> pixelComponentValues = new Dictionary<string, int>();
        public string binaryFilePath = "D:\\VideoTestPatternSample\\binaryfile_8bpp_colorramp-up.bin";
        public string diff_binaryFilePath = "D:\\VideoTestPatternSample\\upd_binaryfile_color-rampup.bin";

        public string textFilePath = "D:\\VideoTestPatternSample\\pixelDataText.txt";
        public string csvfilepath = "D:\\VideoTestPatternSample";
        public string metadata_csvfilepath = "D:\\VideoTestPatternSample\\output.csv";
        

        public int laneValueCount = 4;
        public int horizontalLineCount = 1080;
        public int verticalLineCount = 1920;
        BinaryWriter bw;
        BinaryReader br;



        /// <summary>
        /// This method generates a 24-bit per pixel (Bpp) bitmap image with color bar patterns
        /// </summary>
        public void Bpp_24PldGeneration()
        {
            generate_csvmetaData();
            // List to store generated pixel values
            List<int> generatedPixelValueList = new List<int>();

            // Create a new ConstructImage object to create the image
            ConstructImage objImg = new ConstructImage();
            objImg.CreateBitMap(1900, 1900);

            // Set initial pixel component values to white (255, 255, 255)
            pixelComponentValues.Add("R", 255);
            pixelComponentValues.Add("G", 255);
            pixelComponentValues.Add("B", 255);

            for (int i = 0; i < 256; i++)
            {
                generatedPixelValueList.Add(0);
            }

            using (StreamWriter writer = new StreamWriter(metadata_csvfilepath, true))

            {
                int valueCount = 0; // Track the number of values written in a row

                // Iterate over each horizontal pixel line
                for (int horizontalItr = 0; horizontalItr < horizontalLineCount;)
                {
                    int resetCount = 0;
                    int verticalPatternCount = 1; // To create color bar pattern

                    // Iterate over each vertical pixel line
                    for (int verticalItr = 0; verticalItr < verticalLineCount;)
                    {
                        // If the resetCount reaches the size of 1/8th of verticalLineCount, update the pixelComponentValues for the next color bar pattern
                        if (resetCount == verticalLineCount / 8)
                        {
                            pixelComponentValues.Clear();
                            Update_8bppRGBValue(verticalPatternCount);
                            resetCount = 0;
                            verticalPatternCount++;
                        }

                        resetCount += laneValueCount;

                        // Iterate over each pixel component (R, G, B)
                        for (int cmpItr = 0; cmpItr < 3; cmpItr++)
                        {
                            // Iterate over each lane count value (4R, 4G, 4B in case of L4)
                            for (int laneCntItr = 0; laneCntItr < laneValueCount; laneCntItr++)
                            {
                                // Add pixel data to the list of generated pixel values
                                generatedPixelValueList.Add(pixelComponentValues.ElementAt(cmpItr).Value);
                

                                // Write the pixel value to the CSV file
                                writer.Write(pixelComponentValues.ElementAt(cmpItr).Value);

                                valueCount++;

                                // Check if we have written 4 values in a row
                                if (valueCount == 4)
                                {
                                    writer.WriteLine(); // Start a new line
                                    valueCount = 0; // Reset the value count
                                }
                                else
                                {
                                    writer.Write(","); // Add a comma to separate values in the same row
                                }
                            }
                        }
                        verticalItr += laneValueCount;
                    }



                    // Increment horizontal iterator
                    horizontalItr++;

                    // Clear the pixelComponentValues dictionary to add new color pattern values
                    pixelComponentValues.Clear();
                    Update_8bppRGBValue(0);

                }

                //WriteToFile(generatedPixelValueList);
                generate_binfile(generatedPixelValueList);






            }
        }


        public void Bpp_24PldGeneration_Colorramp()
        {
            const int colorWidth = 256; // Width of each color segment
            const int segmentHeight = 64; // Height of each segment
            generate_csvmetaData();

            // List to store generated pixel values
            List<int> generatedPixelValueList = new List<int>();


            using (StreamWriter writer = new StreamWriter(metadata_csvfilepath, true))

            {
                int valueCount = 0; // Track the number of values written in a row


                // Iterate over each horizontal pixel line
                for (int horizontalItr = 0; horizontalItr < horizontalLineCount; horizontalItr++)
                {

                    // Determine the current color index based on the y-coordinate
                    int colorIndex = (horizontalItr / segmentHeight) % 4;

                    // Iterate over each vertical pixel line
                    for (int verticalItr = 0; verticalItr < verticalLineCount; verticalItr++)
                    {

                        // Calculate the color offset based on the current column
                        int colorOffset = (verticalItr / colorWidth) * colorWidth;

                        // Calculate the color value for the current pixel
                        int colorValue = ((verticalItr - colorOffset) * 256) / colorWidth;

                        //int colorValue = ((verticalItr - colorOffset)  / colorWidth);

                        // Initialize the red, green, and blue components
                        int red = 0;
                        int green = 0;
                        int blue = 0;


                        // Set the color components based on the color index
                        if (colorIndex == 0)
                        {
                            red = colorValue;

                        }
                        else if (colorIndex == 1)
                        {
                            green = colorValue;

                        }
                        else if (colorIndex == 2)
                        {
                            blue = colorValue;
                        }
                        else if (colorIndex == 3)
                        {
                            red = colorValue;
                            green = colorValue;
                            blue = colorValue;

                        }
                        generatedPixelValueList.Add(red);
                        generatedPixelValueList.Add(green);
                        generatedPixelValueList.Add(blue);


                    }

                    // Reset the color index after each set of 4 color segments
                    if (horizontalLineCount % segmentHeight == segmentHeight - 1)
                    {
                        colorIndex = 0;
                    }

                }

                List<int> modifiedPixelValueList = mod_Pixel_Value_List(generatedPixelValueList);
                //WriteToFile(modifiedPixelValueList);
                generate_binfile(modifiedPixelValueList);


                foreach (int pixVal in modifiedPixelValueList)
                    {
                       
                    // Write the pixel value to the CSV file
                    writer.Write(pixVal);

                    valueCount++;

                    // Check if we have written 4 values in a row
                    if (valueCount == 4)
                    {
                        writer.WriteLine(); // Start a new line
                        valueCount = 0; // Reset the value count
                    }
                    else
                    {
                        writer.Write(","); // Add a comma to separate values in the same row
                    }
                }


            }
        }


        public List<int> mod_Pixel_Value_List(List<int> generatedPixelValueList)
        {

            List<int> modifiedPixelValueList = new List<int>();

            int laneCount = 4;
            int componentCount = 3;

            for (int i = 0; i < generatedPixelValueList.Count; i += (laneCount * componentCount))
            {
                for (int j = 0; j < componentCount; j++)
                {
                    for (int k = 0; k < laneCount; k++)
                    {
                        int index = i + (k * componentCount) + j;
                        modifiedPixelValueList.Add(generatedPixelValueList[index]);
                    }
                }
            }

            return modifiedPixelValueList;
        }
        

        public void generate_csvmetaData() 
        
        {
            const int RowCount = 64;
            const int ColumnCount = 4;

            // Open a CSV file for writing
            using (StreamWriter fileWriter = new StreamWriter(metadata_csvfilepath))
            {
                // Write 64 rows to the file
                for (int row = 0; row < RowCount; row++)
                {
                    // Create a string with 4 zeros separated by commas
                    string rowData = string.Join(",", new string[ColumnCount] { "0", "0", "0", "0" });

                    // Write the row to the file
                    fileWriter.WriteLine(rowData);
                }
            }
        }


        public void WriteToFile(List<int> modifiedPixelValueList)

        {
            List<byte> FileContents = modifiedPixelValueList.ConvertAll(b => (byte)b);

            using (FileStream FS = new FileStream($"{binaryFilePath}", FileMode.Append, FileAccess.Write))
            {
                FS.Write(FileContents.ToArray());
                FS.Close();
            }
            FileContents.Clear();

            /*using (BinaryWriter writer = new BinaryWriter(File.Open(binaryFilePath, FileMode.Create)))
            {
                // Convert the byte list to a byte array
                byte[] byteArray = FileContents.ToArray();

                // Write the byte array to the binary file
                writer.Write(byteArray);
            }*/

            Console.WriteLine("Binary file created successfully.");
        }


        public void generate_binfile(List<int> modifiedPixelValueList) {

            // File path of the binary file
            string filePath = diff_binaryFilePath;


        int totalSegments = modifiedPixelValueList.Count  / 4;
         totalSegments = totalSegments + 64;
            int segmentSize = 4;

            //byte[] data = Encoding.ASCII.GetBytes(text);
            // Generate the data for the binary file
            byte[] data = GenerateData(totalSegments * segmentSize);



         int dataCount = modifiedPixelValueList.Count + 256; // Total number of data points
         int groupSize = 4; // Number of data points to update in each segment
         int pixel_counter = 0;

         // Update segments with data groups
         for (int i = 256; i < dataCount; i += groupSize)
         {
             // Calculate the segment index for the group
             int segmentIndex = i / groupSize;

             // Update the bytes in the segment with the data group
             for (int j = 0; j < groupSize; j++)
             {
                 byte pixel_value = Convert.ToByte(modifiedPixelValueList[pixel_counter]);

                 // Update the byte in the segment
                 UpdateByte(data, segmentIndex, j, pixel_value);
                 pixel_counter++;
             }
         }

         // Write the data array to the binary file
         using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
         {
             writer.Write(data);
         }

         Console.WriteLine("Binary file created successfully.");

     // Generate an array of zeros for the data
     static byte[] GenerateData(int length)
     {
         byte[] data = new byte[length];
         return data;
     }

     // Update a specific byte in the data array
     static void UpdateByte(byte[] data, int segmentIndex, int byteIndex, byte newValue)
     {
         int startOffset = segmentIndex * 4;
         int byteOffset = startOffset + byteIndex;
         data[byteOffset] = newValue;
     }



    }


        /// <summary>
        /// This method updates the RGB values of a pixel based on a vertical pattern count(8bpp)
        /// </summary>
        /// <param name="verticarPatternCount"></param>
        private void Update_8bppRGBValue(int verticarPatternCount)
        {
            // Define arrays with pre-set RGB values for each pattern count
            int[] rValues = new int[] { 255, 255, 0, 0, 255, 255, 0, 0 };
            int[] gValues = new int[] { 255, 255, 255, 255, 0, 0, 0, 0 };
            int[] bValues = new int[] { 255, 0, 255, 0, 255, 0, 255, 0 };

            // Check if the pattern count is within the valid range
            if (verticarPatternCount >= 0 && verticarPatternCount <= 7)
            {
                // Clear the existing pixel component values and set the new values based on the pattern count
                pixelComponentValues.Clear();
                pixelComponentValues["R"] = rValues[verticarPatternCount];
                pixelComponentValues["G"] = gValues[verticarPatternCount];
                pixelComponentValues["B"] = bValues[verticarPatternCount];
            }
        }

        static void Main(string[] args)
        {
            Program myObject = new Program();
            myObject.Bpp_24PldGeneration_Colorramp();
        }
    }
}
