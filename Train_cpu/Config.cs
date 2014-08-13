﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Train_DUT
{
    public class Config
    {
        public static string rootPath = @"D:\SemiOnline\Experiment\Nexus\WiFi\channel_54";
        public static string adbPath = @"C:\Users\pok\android\sdk\platform-tools\";
        public static string powerMeterPath = "C:\\Program Files (x86)\\Monsoon Solutions Inc\\PowerMonitor\\PowerToolCmd";
        public static int DUT = 1; //0=nexus, 1=S4
                                             
        
        public static double NEXUS_CPU_POWER_MODEL(double freq, double cpu)
        {
            
            double estPower = 0;

            if(freq == 1000.0)
                estPower = (-1.131 * 0.26) + (6.047 * cpu) + 468.957;

            return estPower;
        }

        public static void callPowerMeter(String savePath, int time)
        {
            Console.WriteLine("Start monsoon\n");

            String pathPowerSave = savePath;

            Process powerMonitor = new Process();
            powerMonitor.StartInfo.FileName = powerMeterPath;
            powerMonitor.StartInfo.Arguments = "/USBPASSTHROUGH=AUTO /VOUT=4.20 /KEEPPOWER /NOEXITWAIT /SAVEFILE=" + pathPowerSave + "  /TRIGGER=DTXD"+time+"A"; //DTYD60A
            powerMonitor.Start();
            powerMonitor.WaitForExit();

            Console.WriteLine("End monsoon\n");
        }

        public static void pullFile(string phonePath, string hostPath)
        {
            string command = adbPath + "adb pull " + phonePath +" "+ hostPath;
            
            Console.WriteLine("Start pull file " + command+"\n");
            
            ProcessStartInfo pullInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            pullInfo.CreateNoWindow = true;
            pullInfo.UseShellExecute = false;
            pullInfo.RedirectStandardError = true;
            pullInfo.RedirectStandardOutput = true;
            Process pullProc = Process.Start(pullInfo);
            

            Console.WriteLine("Finish pull file");
        }

        public static void runAndKillPowerMeter()
        {

            Process powerMonitor = new Process();
            powerMonitor.StartInfo.FileName = "C:\\Program Files (x86)\\Monsoon Solutions Inc\\PowerMonitor\\PowerToolCmd";
            powerMonitor.Start();

            Thread.Sleep(5000);

            powerMonitor.Kill();

            Thread.Sleep(5000);
        }

        public static void callProcess(string command)
        {
           
            command = adbPath + "adb shell \"su -c '" + command + "'\"";
            Console.WriteLine("Start " + command+"\n");

            ProcessStartInfo pInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            pInfo.CreateNoWindow = true;
            pInfo.UseShellExecute = false;
            pInfo.RedirectStandardError = true;
            pInfo.RedirectStandardOutput = true;
            Process process = Process.Start(pInfo);
            StreamReader sOut = process.StandardOutput;
            string result = sOut.ReadLine();            
            Thread.Sleep(3000);
        }

        public static void callProcess2(string command)
        {

            command = adbPath + "adb " + command;
            Console.WriteLine("Start " + command + "\n");

            ProcessStartInfo pInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            pInfo.CreateNoWindow = true;
            pInfo.UseShellExecute = false;
            pInfo.RedirectStandardError = true;
            pInfo.RedirectStandardOutput = true;
            Process process = Process.Start(pInfo);
            StreamReader sOut = process.StandardOutput;
            string result = sOut.ReadLine();
            
            Console.WriteLine("Output = " + result);

            /*try
            {
               
              StreamReader sOut = process.StandardOutput;
               if (!process.HasExited)
                   process.Kill();
               string result = sOut.ReadToEnd();


               if (!process.HasExited)
                   process.Kill();

               int exitCode = process.ExitCode;
               Console.WriteLine("Process output = " + exitCode);
                * 
                * 
               

           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.Message);
           } */
            
           Thread.Sleep(3000);
        }

        public static Boolean isProcessRunning(string name)
        {

            string command = adbPath + "adb shell ps | grep " + name;
            ProcessStartInfo cpuInfo1 = new ProcessStartInfo("cmd.exe", "/c " + command);
            cpuInfo1.CreateNoWindow = true;
            cpuInfo1.UseShellExecute = false;
            cpuInfo1.RedirectStandardError = true;
            cpuInfo1.RedirectStandardOutput = true;
            Process process = Process.Start(cpuInfo1);

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.Close();

            Console.WriteLine("Output = " + output);

            if (output.Length > 0)
                return true;
            else
                return false;
        }

        public static void checkBattStatus(string path)
        {

            //check battery
            ProcessStartInfo battInfo = new ProcessStartInfo("cmd.exe", "/c " + "echo sh -c \" cat /sys/class/power_supply/battery/capacity > /data/local/tmp/batt_cap.txt \" | adb shell");
            battInfo.CreateNoWindow = true;
            battInfo.UseShellExecute = false;
            battInfo.RedirectStandardError = true;
            battInfo.RedirectStandardOutput = true;
            Process battProcess = Process.Start(battInfo);
            Thread.Sleep(2000);
            if (!battProcess.HasExited)
                battProcess.Kill();

            ProcessStartInfo battpullInfo = new ProcessStartInfo("cmd.exe", "/c " + "adb pull /data/local/tmp/batt_cap.txt " + path + "\\batt.txt");
            battpullInfo.CreateNoWindow = true;
            battpullInfo.UseShellExecute = false;
            battpullInfo.RedirectStandardError = true;
            battpullInfo.RedirectStandardOutput = true;
            Process battpullProcess = Process.Start(battpullInfo);
            Thread.Sleep(2000);
            if (!battpullProcess.HasExited)
                battpullProcess.Kill();

            int batt_cap = 0;
            if (File.Exists(path + "\\batt.txt"))
            {
                var lines = File.ReadAllLines(path + "\\batt.txt");
                foreach (var line in lines)
                {
                    batt_cap = int.Parse(line);
                }
            }

            //if (batt_cap <= 50)
            int chargeCap = 100 - batt_cap;

            Console.WriteLine("Charing");
            Thread.Sleep(chargeCap * 30 * 1000); //0.5 min per % 

            Console.WriteLine("Finish charging");
        }

        static double prev_total = 0;
        static double prev_idle = 0;

        public static double parseCPUutil(string cpuData)
        {
                       
            double total = 0;

            string[] cpuElements = cpuData.Split(' ');

            for (int i = 1; i < cpuElements.Length; i++)
            {
                total += Double.Parse(cpuElements[i]);
            }

            double idle = Double.Parse(cpuElements[4]);

            double diff_idle = idle - prev_idle;
            double diff_total = total - prev_total;
            double diff_util = (1000 * (diff_total - diff_idle) / diff_total + 5) / 10;

            prev_total = total;
            prev_idle = idle;

            return diff_util;
        }

        public static double estCpuPower(double util, double freq, double idleTime, double idleEntry)
        {
            double cpuPower = 0;

            if (freq == 200000)
            {
                cpuPower = -0.126 * (idleTime / (idleEntry + 0.01)) + (0.723 * util) + 389.239;
            }
            else if (freq == 400000)
            {
                cpuPower = -0.237 * (idleTime / (idleEntry + 0.01)) + (1.602 * util) + 427.271;
            }
            else if (freq == 800000)
            {
                cpuPower = -0.743 * (idleTime / (idleEntry + 0.01)) + (4.094 * util) + 444.281;
            }
            else if (freq == 1000000)
            {
                cpuPower = -1.131 * (idleTime / (idleEntry + 0.01)) + (6.047 * util) + 468.957;
            }
            else if (freq == 100000)
            {
                cpuPower = (0.2189 * util) + 333.44;
            }
            

            return cpuPower;
        }

        public static double estDisplayPower(double bright)
        {
            return (2.317 * bright) + 0.936;
        }

        public static double estGpuPower(double fps, double t3d_core, double txt_u_l, double usse_ld_ver)
        {
            return (-6.8 * fps) + (-13.4 * t3d_core) + (312.8 * txt_u_l) + (7548.4 * usse_ld_ver) + 71.7;
        }

        public static List<List<string>> processData(string[] d) {

             string[] datas = d;
            
             List<List<string>> lists = new List<List<string>>();
             List<string> list = null;

             int countLoop = 0;
             for (int j = 0; j < datas.Length; j++)
             {
                if (datas[j] == "") continue;

                if (datas[j].Contains("loop"))
                {
                    ++countLoop;
                    Console.WriteLine("Count loop = "+countLoop);
                    if (list != null)
                    {
                        lists.Add(list);
                    }

                    list = new List<string>();
                    list.Add(datas[j]);
                    continue;
                }

                if (!datas[j].Contains("="))
                {
                    continue;
                }

                string[] dats = datas[j].Split('=');
                list.Add(dats[1]);
             }

            //Add last list
            lists.Add(list);
            list = null;
            return lists;
         }

        public static double MAPE(List<double> measure, List<double> model)
        {
            double result = 0;

            double acc = 0;
            int dataSize = measure.Count;
            for (int i = 0; i < dataSize-1; i++)
            {
                if (measure[i] == 0) measure[i] = 0.1;

               double sum = Math.Abs(measure[i] - model[i])/measure[i];

               if (sum > 1) continue;

               //Console.WriteLine("sum = " + sum);
               acc += sum;
            }

            result = (acc / dataSize) * 100;

            return result;

        }

        static int time = 30;

        public static void Run()
        {
            for (int i = 1; i <= time; i++)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            }
        }

        public static void measure()
        {
            string savePath = @"G:\SemiOnline\Experiment\S4\GPU";

            Config.callProcess("rm /data/local/tmp/stat/*.txt");

            if (Config.DUT == 0)
                Config.callProcess("chmod 777 /sys/class/backlight/s5p_bl/brightness");
            else
                Config.callProcess("chmod 777 /sys/class/backlight/panel/brightness");


            Thread.Sleep(5000);

            int numTest = 1;

            for (int i = 1; i <= numTest; i++)
            {

                //if (i == 1)
                //{
                    new Thread(new ThreadStart(Run)).Start();
                //}

                
                if (Config.DUT == 0)
                    Config.callProcess("./data/local/tmp/OGLES2PVRScopeExample " + i + " " + time + " &");

                else
                    Config.callProcess("./data/local/tmp/OGLES2PVRScopeExampleS4 " + i + " " + time + " &");

                Config.callPowerMeter(savePath + @"\power" + i + ".pt4", time);

                Thread.Sleep(10000);

                //Config.pullFile("data/local/tmp/stat/sample" + i + ".txt", savePath);

                //Thread.Sleep(10000);
            }


            Thread.Sleep(30000);

            Config.pullFile("data/local/tmp/stat/", savePath);

            Thread.Sleep(5000);
        }

        static string onPath = "g:\\Semionline\\Experiment\\S4\\on.txt";
        public static void checkConnection()
        {
            Config.callProcess2("pull data/local/tmp/on.txt g:\\Semionline\\Experiment\\S4");
            //Config.callProcess("rm /data/local/tmp/stat/*.txt");
            //Thread.Sleep(5000);

            int count = 0;
            //Check wheather on.txt is existing
            while (!File.Exists(onPath))
            {
                Console.WriteLine("Error count = " + count);
                Config.callProcess2("kill-server");
                Config.callProcess2("start-server");
                Config.callProcess2("pull data/local/tmp/on.txt g:\\Semionline\\Experiment\\S4");
                ++count;

               // if (count >= 10)
                {
                    
                    //SendMail("pokekarat@gmail.com", "pokekarat@gmail.com", "", "S4 is down", "S4 is down");
                 //   Console.Beep(5000, 5000);
                    Console.WriteLine("Have some problem");
                    
                }
            }

            File.Delete(onPath);

        }
        
        public static string SendMail(string toList, string from, string ccList, string subject, string body)
        {

            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();
            string msg = string.Empty;
            try
            {
                System.Net.Mail.MailAddress fromAddress = new System.Net.Mail.MailAddress(from);
                message.From = fromAddress;
                message.To.Add(toList);
                if (ccList != null && ccList != string.Empty)
                    message.CC.Add(ccList);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                // We use gmail as our smtp client
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(
                    "pokekarat", "MAY25199%");

                smtpClient.Send(message);
                msg = "Successful<BR>";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }

        public static void trainS4cpu()
        {

            System.Media.SystemSounds.Asterisk.Play();
            string savePath = @"G:\SemiOnline\Experiment\S4\CPU";
            int numTest = 1;

            // string[] freqs = { "250000", "350000", "450000", "500000", "550000", "600000", "800000", "900000", "1000000", "1100000", "1200000", "1300000", "1400000", "1500000", "1600000" };
            string[] freqs = { /*"250000", "350000", "400000", "600000",*/ "800000", "1200000", "1400000", "1600000" };
            int[] utils = { 25, 50, 75 };
            int[] idleTimes = { 20, 100, 500, 1000 };

            int[] numCoreEnable = { 1 , 2, 3, 4 };

            int index = 1;

            for (int f = 0; f < freqs.Length; f++)
            {
                for (int u = 0; u < utils.Length; u++)
                {
                    for(int it=0; it<idleTimes.Length; it++)
                    {
                        for (int c = 0; c < numCoreEnable.Length; c++)
                        {

                            checkConnection();

                            string freqActive = freqs[f];
                            int utilActive = utils[u];
                            int idleTime = idleTimes[it];
                            int numCoreActive = numCoreEnable[c];

                            //Set cores
                            if (numCoreActive == 1)
                            {  
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu1/online");
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu2/online");
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu3/online");
                            }
                            else if (numCoreActive == 2)
                            {
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu1/online");
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu2/online");
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu3/online");
                            }
                            else if (numCoreActive == 3)
                            {
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu1/online");
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu2/online");
                                callProcess("echo 0 > /sys/devices/system/cpu/cpu3/online");
                            }
                            else if (numCoreActive == 4)
                            {
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu1/online");
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu2/online");
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu3/online");
                            }

                            int y = (utils[u] * (idleTimes[it] * 1000)) / (101 - utils[u]);
                            int x = (idleTimes[it] * 1000) + y;
                         
                            for (int nc = 1; nc <= numCoreActive; nc++)
                            {

                                callProcess("./data/local/tmp/strc "+x+" "+y+" &");

                                /*if (nc == 3)
                                {
                                    callProcess("./data/local/tmp/strc 50000 48000 &");
                                }
                                else
                                {
                                    callProcess("./data/local/tmp/strc 50000 49500 &");
                                }*/
                            }
                                
                                
                            

                            //Set freq
                            for (int nc = 0; nc < numCoreActive; nc++)
                            {
                                callProcess("echo " + freqActive + " > /sys/devices/system/cpu/cpu" + nc + "/cpufreq/scaling_min_freq");
                                callProcess("echo " + freqActive + " > /sys/devices/system/cpu/cpu" + nc + "/cpufreq/scaling_max_freq");
                            }

                            string saveFolder = savePath; // +@"\test_f" + freqActive + "_u" + utilActive + "_c" + numCoreActive;
                        
                            if (!Directory.Exists(saveFolder))
                                Directory.CreateDirectory(saveFolder);

                            for (int i = 1; i <= numTest; i++)
                            {

                                new Thread(new ThreadStart(Run)).Start();
                            
                                Config.callProcess("./data/local/tmp/OGLES2PVRScopeExampleS4 " + index + " " + time + " &");

                                Thread.Sleep(10000);

                                Config.callPowerMeter(saveFolder + @"\t" + index + "_f" + freqActive + "_u" + utilActive + "_c" + numCoreActive + "_idle_" + idleTime + "_" + i + ".pt4", time);

                                //Thread.Sleep(20000);

                                //++index;

                            }

                       
                            //We need to enable this otherwise the system is too busy to do another job.
                            /*if (numCoreActive == 3)
                            {
                                callProcess("echo 1 > /sys/devices/system/cpu/cpu3/online");
                            }*/

                            Thread.Sleep(10000);

                            /* callProcess("chmod 777 data/local/tmp/stat/*.txt");

                             Thread.Sleep(20000);

                             pullFile("data/local/tmp/stat/", saveFolder);

                             Thread.Sleep(5000); 

                             Config.callProcess("rm /data/local/tmp/stat/*.txt");

                             Thread.Sleep(3000); */

                            checkConnection();

                            Config.callProcess("./data/local/tmp/busybox killall strc");

                            Thread.Sleep(5000);

                            //for (int j = index - 3; j < index; j++)
                            
                            {
                                Config.callProcess("chmod 777 data/local/tmp/stat/sample" + index + ".txt");

                                Thread.Sleep(5000);

                                Config.callProcess2("pull data/local/tmp/stat/sample" + index + ".txt g:\\Semionline\\Experiment\\S4\\CPU");

                                //Thread.Sleep(15000);
                            }
                           
                            Thread.Sleep(10000);
                            ++index;
                        }
                    }
                    //pullFile("data/local/tmp/stat/", savePath);
                }
            } 
        }


        // Generate final data of screen
        public static void parseDisplayData()
        {

            string[] brights = { /*"0", "43", "85", "128", "170", "213"};*/  "255" };

            string mergePw = "";
            mergePw = "m c br r g b p\n";

            int offset = 31;
            int stop = 0;
            int start = 0;
            bool isSkipThisLine = false;

            double[] powers = Tool.powerParseArr(@"G:\Semionline\Experiment\S4\Screen\power_255.pt4", offset);

            //string[] powers = File.ReadAllLines(@"G:\Semionline\Experiment\S4\Screen\output\power_output.txt");

            //string[] toSave = Array.ConvertAll(powers, element => element.ToString());

            //File.WriteAllLines(@"G:\Semionline\Experiment\S4\Screen\output\power_output.txt", toSave);

            
            for (int bs = 0; bs < brights.Length; bs++)
            {
                
                Console.WriteLine("write file for "+brights[bs]);

                string[] data = File.ReadAllLines(@"G:\Semionline\Experiment\S4\Screen\screen_color_b_"+brights[bs]+".txt");

                if (bs > 0) offset = 0;

                /*start = start + stop;
                stop = stop + data.Length + offset;

                double[] powers = Tool.powerParseArr(@"G:\Semionline\Experiment\S4\Screen\power.pt4", start + offset, stop, 5000);*/

                //string[] header = data[0].Split('\t');

                double util = 0;
                double its0 = 0;
                double its1 = 0;
                double its2 = 0;
                double ies0 = 0;
                double ies1 = 0;
                double ies2 = 0;

                double m = 0;
                double c = 0;
                
                double br = 0;
                
                int r = 0, g = 0, b = 0;


                for (int i = 0; i < data.Length; i++)
                {
                    // mergePw += data[i] + powers[i+29] + "\n";

                    string[] datas;
                    
                    if(data[i].Contains(' '))
                        datas = data[i].Split(' ');
                    else
                        datas = data[i].Split('\t');


                    for (int j = 0; j < datas.Length; j++)
                    {
                        if (datas[j].Contains("="))
                        {
                            string[] dataEle = datas[j].Split('=');

                            if (dataEle[0] == "u")
                                util = double.Parse(dataEle[1]);

                            else if (dataEle[0] == "its0")
                                its0 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "its1")
                                its1 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "its2")
                                its2 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies0")
                                ies0 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies1")
                                ies1 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies2")
                                ies2 = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "m")
                                m = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "cache")
                                c = double.Parse(dataEle[1]);
                            else if (dataEle[0] == "b")
                            {
                                br = double.Parse(dataEle[1]);
                                if ((int)br != int.Parse(brights[bs]))
                                {

                                    Console.WriteLine("unmatch");
                                    isSkipThisLine = true;
                                    break;
                                   
                                }
                            }
                            else if (dataEle[0] == "rgb")
                            {

                                if (dataEle[1] == "") continue;

                                r = int.Parse(dataEle[1]);

                                g = int.Parse(datas[j + 1]);

                                b = int.Parse(datas[j + 2]);
                            }

                        }
                    }

                    if (isSkipThisLine)
                    {
                        isSkipThisLine = false;
                        continue;
                    }

                    //Pcpu <- input these data to cpu formula to get cpu estimated power of one core cpu
                    double cpuPw = Config.CPU400MHzS4Power(util, its0, its1, its2, ies0, ies1, ies2, 400);

                    double pw = 0;

                    if (!double.IsNaN(cpuPw))
                    {
                        pw = (powers[i]) - cpuPw;
                        Console.WriteLine("NaN");
                    }
                    else
                    {
                        pw = 0;
                        Console.WriteLine("NaN");
                    }

                    
                    //double pwScreen = powers[i + 29] -estCpuPower;
                    //Pscreen <- measure - Pcpu;


                    if (pw < 0)
                    {
                        //Console.WriteLine(m + " " + c + " " + br + " " + r + " " + g + " " + b + " " + pw);
                        pw = 0;
                    }

                    mergePw += m + " " + c + " " + br + " " + r + " " + g + " " + b + " " + pw + "\n";
                   
                   
                }

               
            }

            File.WriteAllText(@"G:\Semionline\Experiment\S4\Screen\output\screen_output2.txt", mergePw);

            mergePw = "";

        }

        public static double CPU400MHzS4Power(double u, double i0, double i1, double i2, double e0, double e1, double e2, double f0)
        {
            double result = -1;

            double idleSum = i0 + i1 + i2;

            if (e0 == 0) e0 = 0.01;
            else if (e1 == 0) e1 = 0.01;
            else if (e2 == 0) e2 = 0.01;


            double constant = 0;

            
            /*if (f0 == 400)
                constant = 716.07;*/
            /*else if (f0 == 600)
                constant = 0;
            else*/

            if (f0 == 400)
            {

                result = 38.21 * ((i0 / idleSum) * i0 / (e0)) +
                         31.56 * ((i1 / idleSum) * i1 / (e1)) +
                         0.76 * ((i2 / idleSum) * i2 / (e2)) +
                         0.38 * u +
                         691; //716.07 for bright = 10
            }
            

            return result;
        }

         // Generate final data of screen
        public static void parseCPUData()
        {

            string[] fileName = { 
                                    "1_idle_1", "1_idle_10","1_idle_50","1_idle_100","1_idle_500","1_idle_1000",
                                    "10_idle_1", "10_idle_10","10_idle_50","10_idle_100","10_idle_500","10_idle_1000",
                                    "25_idle_1", "25_idle_10","25_idle_50","25_idle_100","25_idle_500","25_idle_1000",
                                    "35_idle_1", "35_idle_10","35_idle_50","35_idle_100","35_idle_500","35_idle_1000",
                                    "50_idle_1", "50_idle_10","50_idle_50","50_idle_100","50_idle_500","50_idle_1000",
                                    "60_idle_1", "60_idle_10","60_idle_50","60_idle_100","60_idle_500","60_idle_1000",
                                    "100_idle_1", "100_idle_10","100_idle_50","100_idle_100","100_idle_500","100_idle_1000"
                                };

            string header = "u its0 its1 its2 ies0 ies1 ies2 m c power\n";

            string toPrint = header;

            int fileLen = fileName.Length;
            for (int f = 0; f < fileLen; f++)
            {


                Console.WriteLine("Finish "+f+":"+fileLen);

                if (!File.Exists(@"G:\Semionline\Experiment\S4\CPU_old\CPU_one_core\old\CPU_idle_400_to_800\test_1_freq_400000_util_" + fileName[f] + ".txt")) continue;

                string[] data = File.ReadAllLines(@"G:\Semionline\Experiment\S4\CPU_old\CPU_one_core\old\CPU_idle_400_to_800\test_1_freq_400000_util_"+fileName[f]+".txt");
                double[] powerData = Tool.powerParseArr(@"G:\Semionline\Experiment\S4\CPU_old\CPU_one_core\old\CPU_idle_400_to_800\test_1_freq_400000_util_" + fileName[f] + ".pt4", 20, data.Length, 5000);
                
                double util = 0;
                double its0 = 0;
                double its1 = 0;
                double its2 = 0;
                double ies0 = 0;
                double ies1 = 0;
                double ies2 = 0;
                double m = 0;
                double c = 0;
                double power = 0;

                int begin = 20;
                int end = powerData.Length;
                int end2 = data.Length;

                int minEnd = Math.Min(end, end2);
                for (int i = begin; i < minEnd; i++)
                {

                    string line = data[i];
                    string[] elements = line.Split(' ');

                    for (int j = 0; j < elements.Length; j++)
                    {
                        if (elements[j].Contains("="))
                        {
                            string[] dataEle = elements[j].Split('=');

                            if (dataEle[0] == "u")
                                util += double.Parse(dataEle[1]);

                            else if (dataEle[0] == "its0")
                                its0 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "its1")
                                its1 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "its2")
                                its2 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies0")
                                ies0 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies1")
                                ies1 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "ies2")
                                ies2 += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "m")
                                m += double.Parse(dataEle[1]);
                            else if (dataEle[0] == "cache")
                                c += double.Parse(dataEle[1]);



                            /*if (dataEle[0] == "u" || dataEle[0] == "its0" || dataEle[0] == "its1" || dataEle[0] == "its2" || dataEle[0] == "ies0" ||dataEle[0] == "ies1" ||
                                dataEle[0] == "ies2" ||dataEle[0] == "m" ||dataEle[0] == "cache")
                            {
                                print += dataEle[1] + " ";
                            }

                            if (dataEle[0] == "cache")
                            {
                                print += powerData[j] + "\n";
                            }*/
                        }

                    }

                    power += powerData[i];
                }

                int size = end - begin;
                double avgUtil = util / size;
                double avgits0 = its0 / size;
                double avgits1 = its1 / size;
                double avgits2 = its2 / size;
                double avgies0 = ies0 / size;
                double avgies1 = ies1 / size;
                double avgies2 = ies2 / size;
                double avgM = m / size;
                double avgC = c / size;
                double avgPower = power / size;

                string output = avgUtil + " " + avgits0 + " " + avgits1 + " " + avgits2 + " " + avgies0 + " " + avgies1 + " " + avgies2 + " " + avgM + " " + avgC + " " + avgPower;

                toPrint += output + "\n";
            }

            File.WriteAllText(@"G:\Semionline\Experiment\S4\CPU_old\CPU_one_core\output\train_400.txt", toPrint);
            
        }
    }
}
