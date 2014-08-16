﻿
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

namespace Train_DUT
{

    class Program
    {
        static void Main(string[] args)
        {

            int mode = 7; // 1=train, 2=evaluate, 3=screen, 4 = gps

            if (mode == 12)
            {
                //evalScreen.execute();
                Config.parseCPUData();
            }

          

            if (mode == 10)
            {
                Config.parseDisplayData();
            }


            if (mode == 9)
            {
                //Config.measure();
                Config.trainS4cpu();
            }

            else if (mode == 8)
            {
                //testApp.parseNexusS();
                testApp.parseS4();
            }

            //Train
            else if (mode == 1)
            {
                Train_CPU ts = new Train_CPU(2);
                ts.execute();
            }

            else if (mode == 2)
            {

            }

            else if (mode == 3)
            {
                new evalCPU().execute();
            }

            else if (mode == 4)
            {
                new evalGPS().execute2();
            }

            else if (mode == 5)
            {
                new evalWiFi().execute2("36");
            }

            else if (mode == 6)
            {
                //Tool.powerPartition(Config.rootPath+@"\power", 30, 264);
                string[] channel = Config.rootPath.Split('_');
                new evalWiFi().execute2(channel[1]);
            }

            else if (mode == 7)
            {
               evalGPU eg = new evalGPU();
               
               eg.Evaluate2();
            }
            

            Console.WriteLine("Finish.");
            System.Media.SystemSounds.Asterisk.Play();
            Console.ReadKey();
        }
    }
}
