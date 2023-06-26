using GRL.Utils.C2VEnums.DPEnum.Enums;
using GRL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelimageGeneration
{
    public static class DecodeData
    {

        /// <summary>
        /// decode 18bits pixel data
        /// </summary>
        /// <param name="cfgLaneCount"></param>
        /// <param name="rawDataLst"></param>
        public static List<Pixel> RGB_6bpp(int cfgLaneCount, List<int> rawDataLst)
        {
            int Count = ((rawDataLst.Count * 8) / 18);
            List<Pixel> PixelInfo = new List<Pixel>();
            for (int i = 0; i < Count + 10; i++)
            {
                PixelInfo.Add(new Pixel()
                {
                    Info = new Dictionary<RGBcomponent, byte>()
                {
                    {RGBcomponent.R, 0 },{ RGBcomponent.G, 0 },{RGBcomponent.B, 0  }
                }
                });
            }
            try
            {
                RGBcomponent FirstCmp = RGBcomponent.R;//LSB
                RGBcomponent SecondCmp = RGBcomponent.G;//MSB
                int NoOfBitsDec = 0;
                int ExpDecBitCnt = 18;//18 bpp
                int FirstCmpBitCnt = 6, SecCmpBitCnt = 2;
                int PresInd = 0;
                int NextInd = 0;
                //Remove raw data lst or define iterator accordingly
                //Define break                    
                int Itr = 0;
                while (Itr < rawDataLst.Count)
                {
                    //Updated Pixel index
                    NoOfBitsDec += FirstCmpBitCnt;
                    PresInd = UpdatePixelIndex(cfgLaneCount, ref NoOfBitsDec, ExpDecBitCnt, ref NextInd, ref PresInd, out bool IndUpdMSBPart);
                    bool IndUpdLSBPart = false;
                    if (IndUpdMSBPart == false)
                    {
                        NoOfBitsDec += SecCmpBitCnt;
                        PresInd = UpdatePixelIndex(cfgLaneCount, ref NoOfBitsDec, ExpDecBitCnt, ref NextInd, ref PresInd, out IndUpdLSBPart);
                    }
                    if (IndUpdMSBPart)
                        NoOfBitsDec += SecCmpBitCnt;
                    else if (IndUpdLSBPart)
                    {
                        //NoOfBitsDec = 8;//TODO:: Check whether its required
                        NextInd = PresInd;
                    }

                    byte SecCmpAndOp = (byte)((1 << SecCmpBitCnt) - 1);
                    byte FrstCmpAndOp = (byte)((1 << FirstCmpBitCnt) - 1);
                    int Val1 = 0, Val2 = 0;
                    for (int j = 0; j < cfgLaneCount; j++)
                    {
                        //MSB of second component will be in LSB of current byte
                        Val1 = (rawDataLst[Itr] & SecCmpAndOp);
                        Val2 = (rawDataLst[Itr] >> SecCmpBitCnt);
                        if ((NextInd + j) >= PixelInfo.Count)
                        {

                        }
                        PixelInfo[NextInd + j].Info[SecondCmp] |= (byte)((rawDataLst[Itr] & SecCmpAndOp) << (6 - SecCmpBitCnt));
                        if ((PresInd + j) >= PixelInfo.Count)
                        {

                        }
                        PixelInfo[PresInd + j].Info[FirstCmp] |= (byte)((rawDataLst[Itr] >> SecCmpBitCnt) & FrstCmpAndOp);
                        Itr++;
                    }

                    //Update component to next 
                    if (SecCmpBitCnt == 6)//If prev component is completely decoded
                    {
                        FirstCmp = GetNextCmp(SecondCmp);
                    }
                    else
                        FirstCmp = SecondCmp;
                    SecondCmp = GetNextCmp(FirstCmp);

                    //Update bit split info
                    if ((FirstCmpBitCnt - 2) >= 2)
                        FirstCmpBitCnt -= 2;
                    else
                        FirstCmpBitCnt = 6;
                    SecCmpBitCnt = 8 - FirstCmpBitCnt;

                    if (IndUpdLSBPart)
                    {
                        NextInd += cfgLaneCount;
                        PresInd = NextInd;
                    }
                    //if ((SecCmpBitCnt + 2) <= 6)
                    //    SecCmpBitCnt += 2;
                    //else
                    //    SecCmpBitCnt = 2;
                }
            }
            catch (Exception ex)
            {
            }
            return PixelInfo;
        }

        /// <summary>
        /// decode 24bits pixel data
        /// </summary>
        /// <param name="cfgLaneCount"></param>
        /// <param name="rawDataLst"></param>
        public static List<Pixel> RGB_8bpp(int cfgLaneCount, List<int> rawDataLst)
        {
            List<Pixel> PixelInfo = new List<Pixel>();
            try
            {
                int Length = 0;
                Length = cfgLaneCount * 3;
                while (true)
                {
                    //Console.WriteLine(" .....");
                    if (rawDataLst.Count < Length)
                        break;
                    List<Pixel> PxlLst = new List<Pixel>();
                    for (int i = 0; i < cfgLaneCount; i++)
                    {
                        PxlLst.Add(new Pixel());
                    }
                    RGBcomponent CurrCmp = RGBcomponent.R;
                    int Itr = 0;
                    for (int k = 0; k < 3; k++)//R G B components
                    {
                        for (int l = 0; l < PxlLst.Count; l++)// Number of pixel set as per lane count
                        {
                            PxlLst[l].Info[CurrCmp] = (byte)rawDataLst[Itr];
                            Itr++;
                        }
                        CurrCmp++;
                    }
                    PixelInfo.AddRange(PxlLst);
                    rawDataLst.RemoveRange(0, Length);
                }
            }
            catch (Exception ex)
            {

            }
            return PixelInfo;

        }

        /// <summary>
        /// decode 30bits pixel data
        /// </summary>
        /// <param name="laneValueCount"></param>
        /// <param name="generatedPixelData"></param>
        public static List<Pixel> RGB_10bpp(int laneValueCount, List<int> generatedPixelData)
        {
            List<int> decodedPixelData = new List<int>();
            List<int> decodedDataLane0 = new List<int>();
            List<int> decodedDataLane1 = new List<int>();
            List<int> decodedDataLane2 = new List<int>();
            List<int> decodedDataLane3 = new List<int>();

            //decode 30 bits per pixel
            for (int GenPixItr = 0; GenPixItr < generatedPixelData.Count;)
            {
                for (int LaneCntItx = 0; LaneCntItx < laneValueCount; LaneCntItx++)
                {
                    int ind1 = generatedPixelData.ElementAt(GenPixItr);
                    int ind2 = generatedPixelData.ElementAt(GenPixItr + 1);
                    int ind3 = generatedPixelData.ElementAt(GenPixItr + 2);
                    int ind4 = generatedPixelData.ElementAt(GenPixItr + 3);
                    int ind5 = generatedPixelData.ElementAt(GenPixItr + 4);
                    int decodedIndex1 =((ind1 << 2 | ind2 >> 6) & 0X3FF);
                    int decodedIndex2 = ((ind2 << 4 | ind3 >> 4) & 0X3FF);
                    int decodedIndex3 = ((ind3 << 6 | ind4 >> 2) & 0X3FF);
                    int decodedIndex4 = ((ind4 << 8 | ind5) & 0X3FF);

                    if (LaneCntItx == 0)
                    {
                        decodedDataLane0.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });
                    }
                    if (LaneCntItx == 1)
                    {
                        decodedDataLane1.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }
                    if (LaneCntItx == 2)
                    {
                        decodedDataLane2.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }
                    if (LaneCntItx == 3)
                    {
                        decodedDataLane3.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }

                    GenPixItr += 5;
                }

            }

            if (laneValueCount == 1)
            {
                    decodedPixelData.AddRange(decodedDataLane0);
                
            }
           
            if (laneValueCount == 2)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                    }
                }
            }
            if (laneValueCount == 4)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane2[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane3[decodeItr + rgbCmpItx]);
                    }
                }
            }
            List<Pixel> PixelInfo =Filter_Decoded_Data(laneValueCount, decodedPixelData);
            return PixelInfo;
        }

        /// <summary>
        /// decode 36bits pixel data
        /// </summary>
        /// <param name="laneValueCount"></param>
        /// <param name="generatedPixelData"></param>
        public static List<Pixel> RGB_12bpp(int laneValueCount, List<int> generatedPixelData)
        {
            List<int> decodedPixelData = new List<int>();
            List<int> decodedDataLane0 = new List<int>();
            List<int> decodedDataLane1 = new List<int>();
            List<int> decodedDataLane2 = new List<int>();
            List<int> decodedDataLane3 = new List<int>();

            //decode 36 bits per pixel
            for (ushort GenPixItr = 0; GenPixItr < generatedPixelData.Count;)
            {
                for (int LaneCntItx = 0; LaneCntItx < laneValueCount; LaneCntItx++)
                {
                    int ind1 = generatedPixelData.ElementAt(GenPixItr);
                    int ind2 = generatedPixelData.ElementAt(GenPixItr + 1);
                    int ind3 = generatedPixelData.ElementAt(GenPixItr + 2);
                    int ind4 = generatedPixelData.ElementAt(GenPixItr + 3);
                    int ind5 = generatedPixelData.ElementAt(GenPixItr + 4);
                    int ind6 = generatedPixelData.ElementAt(GenPixItr + 5);
                    int decodedIndex1 = ((ind1 << 4 | ind2 >> 4) & 0XFFF);
                    int decodedIndex2 = ((ind2 << 8 | ind3) & 0XFFF);
                    int decodedIndex3 = ((ind4 << 4 | ind5 >> 4) & 0XFFF);
                    int decodedIndex4 = ((ind5 << 8 | ind6) & 0XFFF);
                    if (LaneCntItx == 0)
                    {
                        decodedDataLane0.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });
                    }
                    if (LaneCntItx == 1)
                    {
                        decodedDataLane1.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }
                    if (LaneCntItx == 2)
                    {
                        decodedDataLane2.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }
                    if (LaneCntItx == 3)
                    {
                        decodedDataLane3.AddRange(new List<int> { decodedIndex1, decodedIndex2, decodedIndex3, decodedIndex4 });

                    }

                    GenPixItr += 6;
                }

            }

            if (laneValueCount == 1)
            {
                decodedPixelData.AddRange(decodedDataLane0);
            }

            if (laneValueCount == 2)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                    }
                }
            }
            if (laneValueCount == 4)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane2[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane3[decodeItr + rgbCmpItx]);
                    }
                }
            }
            List<Pixel> PixelInfo = Filter_Decoded_Data(laneValueCount, decodedPixelData);
            return PixelInfo;
        }

        /// <summary>
        /// decode 48bits pixel data
        /// </summary>
        /// <param name="laneValueCount"></param>
        /// <param name="generatedPixelData"></param>
        public static List<Pixel> RGB_16bpp(int laneValueCount, List<int> generatedPixelData)
        {
            List<int> decodedPixelData = new List<int>();
            List<int> decodedDataLane0 = new List<int>();
            List<int> decodedDataLane1 = new List<int>();
            List<int> decodedDataLane2 = new List<int>();
            List<int> decodedDataLane3 = new List<int>();

            //decode 36 bits per pixel
            for (int GenPixItr = 0; GenPixItr < generatedPixelData.Count;)
            {
                for (int LaneCntItx = 0; LaneCntItx < laneValueCount; LaneCntItx++)
                {
                    int ind1 = generatedPixelData.ElementAt(GenPixItr);
                    int ind2 = generatedPixelData.ElementAt(GenPixItr);

                    int decodedIndex = (ind2 << 8 | ind1);

                    if (LaneCntItx == 0)
                    {
                        decodedDataLane0.Add(decodedIndex);
                    }
                    if (LaneCntItx == 1)
                    {
                        decodedDataLane1.Add(decodedIndex);

                    }
                    if (LaneCntItx == 2)
                    {
                        decodedDataLane2.Add(decodedIndex);

                    }
                    if (LaneCntItx == 3)
                    {
                        decodedDataLane3.Add(decodedIndex);

                    }

                    GenPixItr += 2;
                }

            }

            if (laneValueCount == 1)
            {
                decodedPixelData.AddRange(decodedDataLane0);
            }

            if (laneValueCount == 2)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                    }
                }
            }
            if (laneValueCount == 4)
            {
                for (int decodeItr = 0; decodeItr < decodedDataLane0.Count; decodeItr += 3)
                {
                    for (int rgbCmpItx = 0; rgbCmpItx < 3; rgbCmpItx++)
                    {
                        decodedPixelData.Add(decodedDataLane0[decodeItr + rgbCmpItx]);

                        decodedPixelData.Add(decodedDataLane1[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane2[decodeItr + rgbCmpItx]);
                        decodedPixelData.Add(decodedDataLane3[decodeItr + rgbCmpItx]);
                    }
                }
            }
            List<Pixel> PixelInfo = Filter_Decoded_Data(laneValueCount,decodedPixelData);
            return PixelInfo;
        }

        public static List<Pixel> Filter_Decoded_Data(int laneValueCount,List<int> decodedPixelData) {

            List<Pixel> PixelInfo = new List<Pixel>();
            try
            {
                int Length = 0;
                Length = laneValueCount * 3;
                while (true)
                {
                    if (decodedPixelData.Count < Length)
                        break;
                    List<Pixel> PxlLst = new List<Pixel>();
                    for (int i = 0; i < laneValueCount; i++)
                    {
                        PxlLst.Add(new Pixel());
                    }
                    RGBcomponent CurrCmp = RGBcomponent.R;
                    int Itr = 0;
                    for (int k = 0; k < 3; k++)//R G B components
                    {
                        for (int l = 0; l < PxlLst.Count; l++)// Number of pixel set as per lane count
                        {
                            PxlLst[l].Info[CurrCmp] = (byte)decodedPixelData[Itr];
                            Itr++;
                        }
                        CurrCmp++;
                    }
                    PixelInfo.AddRange(PxlLst);
                    decodedPixelData.RemoveRange(0, Length);
                }
            }
            catch (Exception ex)
            {

            }
            return PixelInfo;




        }
        public static int UpdatePixelIndex(int cfgLaneCount, ref int NoOfBitsDec, int ExpDecBitCnt, ref int NextInd, ref int PresInd, out bool indUpd)
        {
            indUpd = false;
            if ((NoOfBitsDec) >= ExpDecBitCnt)
            {
                //PresInd = NextInd;
                NoOfBitsDec -= 18;
                NextInd += cfgLaneCount;
                //NoOfBitsDec = 0;
                indUpd = true;
            }
            else
                PresInd = NextInd;
            return PresInd;
        }

        public static RGBcomponent GetNextCmp(RGBcomponent currCmp)
        {
            switch (currCmp)
            {
                case RGBcomponent.R:
                    return RGBcomponent.G;
                case RGBcomponent.G:
                    return RGBcomponent.B;
                case RGBcomponent.B:
                    return RGBcomponent.R;
            }
            return currCmp;
        }




    }


}

