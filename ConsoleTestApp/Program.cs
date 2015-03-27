﻿/* 
 * FileName:    Program.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  3/11/2015 10:19:04 AM
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelperLibrary.Core;
using HelperLibrary.Core.ExtensionHelper;
using HelperLibrary.Core.Configurations;
using System.IO;
using HelperLibrary.Core.Tree;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //NumberUtilityTest();
            //StringExtensionsTest();
            //XmlConfigTest();
            //StringUtilityTest();
            TreeNodeTest();

            Console.ReadKey();
        }

        private static void TreeNodeTest()
        {
            string rootPath = @"E:\E_Books";
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);

            TreeNode<FileSystemInfo> root = new TreeNode<FileSystemInfo>() { Value = rootDir };

            BuildTree(rootDir, root);
            root.DepthFirstTraversal(node => Console.WriteLine(node.Value.FullName));
            Console.WriteLine("=============================================");
            root.BreadthFirstTraversal(node => Console.WriteLine(node.Value.FullName));
        }

        private static void BuildTree(DirectoryInfo rootDir, TreeNode<FileSystemInfo> root)
        {
            foreach (var item in rootDir.GetFileSystemInfos())
            {
                var file = item as FileInfo;
                if (file != null)
                {
                    root.AddChild(new TreeNode<FileSystemInfo>() { Value = file });
                }
                else
                {
                    var dir = item as DirectoryInfo;
                    var dirNode = new TreeNode<FileSystemInfo>() { Value = dir };
                    BuildTree(dir, dirNode);
                    root.AddChild(dirNode);
                }
            }
        }

        private static void StringUtilityTest()
        {
            string str = "上下st";
            try
            {
                Console.WriteLine(StringUtility.GetMD5OfString(str));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static void XmlConfigTest()
        {
            string filePath = @"cfg.xml";
            bool isCreateNew = !System.IO.File.Exists(filePath);

            IConfigurationFile cfg = new XmlConfigurationFile(filePath, isCreateNew);
            cfg["time"] = DateTime.Now.ToString();

            Console.WriteLine(cfg["time"]);
            Console.WriteLine(cfg.GetConfiguration("time"));

            //this clear all changes
            cfg.Reload();

            //throw exception
            //cfg.UpdateConfiguration("update", "test");

            cfg.SaveChange();
        }

        private static void StringExtensionsTest()
        {
            string str = "abcdefg1235";
            Console.WriteLine("original string: " + str);
            Console.WriteLine("reverse string: " + str.ReverseString().FirstCharToUpper());
            Console.WriteLine("first char to upper:" + str.FirstCharToUpper());
        }

        private static void NumberUtilityTest()
        {
            // random int
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("random number:" + NumberUtility.GetRandomInt(-128, 127).ToString());
            }


            // hex string
            byte[] bytes = new byte[] { 20, 56, 9, 10, 11, 15, 16, 56 };
            Console.WriteLine("hex string:" + NumberUtility.BytesToHexString(bytes, true));

        }
    }
}
