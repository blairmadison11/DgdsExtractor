using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DgdsExtractor
{
    /*
     * This code is taken and modified from https://github.com/pevillarreal/LzwCompressor
     * 
     * Used with permission by the following MIT License
    
        Copyright (c) 2019 Pedro Villarreal

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
    */

	class LzwDecompressor
	{
        private const int MAX_BITS = 14; //maimxum bits allowed to read
        private const int HASH_BIT = MAX_BITS - 8; //hash bit to use with the hasing algorithm to find correct index
        private const int MAX_VALUE = (1 << MAX_BITS) - 1; //max value allowed based on max bits
        private const int MAX_CODE = MAX_VALUE - 1; //max code possible
        private const int TABLE_SIZE = 18041; //must be bigger than the maximum allowed by maxbits and prime

        private int[] _iaCodeTable = new int[TABLE_SIZE]; //code table
        private int[] _iaPrefixTable = new int[TABLE_SIZE]; //prefix table
        private int[] _iaCharTable = new int[TABLE_SIZE]; //character table

        private ulong _iBitBuffer; //bit buffer to temporarily store bytes read from the files
        private int _iBitCounter; //counter for knowing how many bits are in the bit buffer

        private void Initialize() //used to blank  out bit buffer incase this class is called to comprss and decompress from the same instance
        {
            _iBitBuffer = 0;
            _iBitCounter = 0;
        }

        public byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream outData = new MemoryStream();
            BinaryWriter output = new BinaryWriter(outData);

            Initialize();

            int iNextCode = 256;
            int iNewCode, iOldCode;
            byte bChar;
            int iCurrentCode, iCounter;
            byte[] baDecodeStack = new byte[TABLE_SIZE];

            iOldCode = ReadCode(input);
            bChar = (byte)iOldCode;
            output.Write((byte)iOldCode); //write first byte since it is plain ascii

            iNewCode = ReadCode(input);

            while (iNewCode != MAX_VALUE) //read file all file
            {
                if (iNewCode >= iNextCode)
                { //fix for prefix+chr+prefix+char+prefx special case
                    baDecodeStack[0] = bChar;
                    iCounter = 1;
                    iCurrentCode = iOldCode;
                }
                else
                {
                    iCounter = 0;
                    iCurrentCode = iNewCode;
                }

                while (iCurrentCode > 255) //decode string by cycling back through the prefixes
                {
                    //lstDecodeStack.Add((byte)_iaCharTable[iCurrentCode]);
                    //iCurrentCode = _iaPrefixTable[iCurrentCode];
                    baDecodeStack[iCounter] = (byte)_iaCharTable[iCurrentCode];
                    ++iCounter;
                    if (iCounter >= MAX_CODE)
					{
                        Console.WriteLine("***DECOMPRESSION ERROR***");
                        return data;
                    }
                        
                    iCurrentCode = _iaPrefixTable[iCurrentCode];
                }

                baDecodeStack[iCounter] = (byte)iCurrentCode;
                bChar = baDecodeStack[iCounter]; //set last char used

                while (iCounter >= 0) //write out decodestack
                {
                    output.Write(baDecodeStack[iCounter]);
                    --iCounter;
                }

                if (iNextCode <= MAX_CODE) //insert into tables
                {
                    _iaPrefixTable[iNextCode] = iOldCode;
                    _iaCharTable[iNextCode] = bChar;
                    ++iNextCode;
                }

                iOldCode = iNewCode;

                iNewCode = ReadCode(input);
            }

            output.Flush();
            return outData.ToArray();
        }

        private int ReadCode(Stream pReader)
        {
            uint iReturnVal;

            while (_iBitCounter <= 24) //fill up buffer
            {
                _iBitBuffer |= (ulong)pReader.ReadByte() << (24 - _iBitCounter); //insert byte into buffer
                _iBitCounter += 8; //increment counter
            }

            iReturnVal = (uint)_iBitBuffer >> (32 - MAX_BITS); //get last byte from buffer so we can return it
            _iBitBuffer <<= MAX_BITS; //remove it from buffer
            _iBitCounter -= MAX_BITS; //decrement bit counter

            int temp = (int)iReturnVal;
            return temp;
        }
    }
}
