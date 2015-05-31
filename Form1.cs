using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PMDEditor;
using System.IO;
using LibMMM;

namespace PMXCheckerForOMP
{
    public partial class Form1 : Form
    {
        #region PMXChecker Header
        PmxFile p = new PmxFile();
        Pmx PModel = new Pmx();
        Dictionary<string, int> BoneMap = new Dictionary<string, int>();
        Dictionary<int, string> BoneIK = new Dictionary<int, string>();
        Dictionary<string, int> IKMap = new Dictionary<string, int>();
        Dictionary<string, List<string>> ErrList = new Dictionary<string, List<string>>();
        string BaseFile = "";
        string TargetFile = "";
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        #region PMXChecker Body
        void LoadPmd(string FormFile)
        {
            textBox1.Text = "";
            System.IO.FileInfo fi=new FileInfo(FormFile);
            if (fi.Extension.ToLower() != ".pmx")
            {
                MessageBox.Show("检查器只处理PMX文件！", "Format Error");
            }
            else
            {
                Print("==========================");
                Print("读取模型文件:" + FormFile);

                PModel = p.GetFile(FormFile);//@"H:\代传\TDA校服\Tda School SeeU\SeeU School.pmx");
                BoneMap.Clear();
                IKMap.Clear();
                BoneIK.Clear();
                ErrList.Clear();
                BaseFile = FormFile;
                TargetFile = FormFile.Substring(0, FormFile.Length - 4) + "_forOMP.pmx";
                Print("处理结束！");
                Print("==========================");
            }

            ErrList.Clear();
            ErrList = Check();
        }
        void Print(string str)
        {
            int newLength=str.Length;
            if (textBox1.Text.Length + newLength + 2 > textBox1.MaxLength)
            {
                string[] sl = textBox1.Lines;
                int Dert = textBox1.Text.Length + newLength + 2 - textBox1.MaxLength;
                int DelIndex=-1;
                int LenD = 0;
                while (Dert>LenD && DelIndex<sl.Length)
                {
                    DelIndex++;
                    LenD = LenD + sl[DelIndex].Length;
                }
                StringBuilder newSB = new StringBuilder();
                for (int i = DelIndex; i < sl.Length; i++)
                {
                    try
                    {
                        newSB.AppendLine(sl[i]);
                    }
                    catch { ;}
                }
                textBox1.Text = newSB.ToString();
            }
            textBox1.Text = textBox1.Text + "\r\n" + str;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "打开MMD模型";
            ofd.AddExtension = true;
            ofd.DefaultExt = ".pmx";
            ofd.Filter = "*.pmx|*.pmx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadPmd(ofd.FileName);
            }
        }
        void FixErr()
        {
            Print("OMP用MMD模型编辑输出开始");
            List<string> MissingBone = ErrList["丢失骨骼"];
            List<string> MissingIK = ErrList["丢失显示枠"];
            List<string> ErrBone = ErrList["错误IK"];
            #region 骨骼
            if (MissingBone.Count > 0)
            {
                Print("==========================");
                Print("发现丢失骨骼，但程序不能添加，你需要使用PmxEditor来使用它们。也可能是你设置的骨骼名称不符合要求。");
                Print("丢失骨骼如下：");
                foreach (string s in MissingBone)
                {
                    Print("\ts");
                }
            }
            #endregion
            #region 检查枠
            if (MissingIK.Count > 0)
            {
                Print("==========================");
                Print("发现需要添加的显示枠");
                foreach (string s in MissingIK)
                {
                    try
                    {
                        Print("\t添加显示枠:" + s);
                        PmxNode.NodeElement Item = new PmxNode.NodeElement();
                        Item.ElementType = PmxNode.ElementType.Bone;
                        Item.Index = BoneMap[s];
                        PModel.NodeList[0].ElementList.Add(Item);
                        Print("\t\t添加成功");
                    }
                    catch { Print("\t\t添加失败"); }
                }
            }
            #endregion
            #region 检查骨骼
            if (ErrBone.Count > 0)
            {
                Print("==========================");
                Print("发现需要矫正的特殊骨骼");
                foreach (string s in ErrBone)
                {
                    if (s.Length>3 && s.Substring(0, 3) == "MIS")
                    {
                        string ns = s.Substring(3);
                        Print("\t添加丢失骨骼："+ns);
                        PMDEditor.PmxBone newBone = p.CreateNewBone();
                        try
                        {
                            switch (ns)
                            {
                                case "左つま先":
                                    newBone.Name = "左つま先";
                                    newBone.Position.X = 1.013f;
                                    newBone.Position.Y = 0.5710342f;
                                    newBone.Position.Z = -1.14298f;
                                    newBone.Parent = BoneMap["左足首"];
                                    break;
                                case "右つま先":
                                    newBone.Name = "右つま先";
                                    newBone.Position.X = -1.013f;
                                    newBone.Position.Y = 0.571034f;
                                    newBone.Position.Z = -1.14298f;
                                    newBone.Parent = BoneMap["右足首"];
                                    break;
                            }
                            if (newBone.Name != "")
                            {
                                PModel.BoneList.Add(newBone);
                                BoneMap.Add(newBone.Name, PModel.BoneList.Count - 1);
                                Print("\t\t添加成功！");
                            }else
                            {
                                Print("\t\t添加失败！");
                            }
                        }catch{Print("\t\t发生未知错误");}

                    }
                    else
                    {
                        Print("\t重置骨骼设置：" + s);
                        int BIK = BoneMap[s];
                        PModel.BoneList[BIK].Flags = PModel.BoneList[BIK].Flags | PMDEditor.PmxBone.BoneFlags.ToBone;
                        PModel.BoneList[BIK].To_Offset.X = 0;
                        PModel.BoneList[BIK].To_Offset.Y = 0;
                        PModel.BoneList[BIK].To_Offset.Z = 0;
                        PModel.BoneList[BIK].To_Bone = -1;
                        Print("\t\t设置成功！");
                    }
                }
                ErrBone.Clear();
            }
            #endregion
            Print("\r\n==========================");
            Print("写入输出文件:"+TargetFile);
            PModel.ToFile(TargetFile);
            Print("处理结束！");
            Print("==========================");
        }
        Dictionary<string, List<string>> Check()
        {
            Print("OMP用MMD模型检查开始");
            Print("==========================");
            Print("\t正在准备数据...骨骼索引");
            BoneMap.Clear();
            for (int i = 0; i < PModel.BoneList.Count; i++)
            {
                BoneMap.Add(PModel.BoneList[i].Name, i);
            }

            Print("\t正在准备数据...IK索引");
            for (int i = 0; i < PModel.NodeList.Count; i++)
            {
                try
                {
                    string IKN = "";
                    while (IKMap.ContainsKey(PModel.NodeList[i].Name + IKN))
                    {
                        if (IKN == "") IKN = "0";
                        IKN = (int.Parse(IKN) + 1).ToString();
                    }
                    IKMap.Add(PModel.NodeList[i].Name + IKN, i);
                    for (int j = 0; j < PModel.NodeList[i].ElementList.Count; j++)
                    {
                        if (PModel.NodeList[i].ElementList[j].ElementType == PmxNode.ElementType.Bone)
                        {
                            try
                            {
                                BoneIK.Add(PModel.NodeList[i].ElementList[j].Index, PModel.NodeList[i].Name + IKN);
                            }
                            catch { ;}
                        }
                    }
                }
                catch { ;}
            }


            Dictionary<string, List<string>> Result = new Dictionary<string, List<string>>();
            Print("==========================");
            Print("正在检查定帧骨骼");
            bool allRight = true;
            List<string> MissingBones = new List<string>();
            for (int i = 0; i < Table.MustBones.Count; i++)
            {
                if (!BoneMap.ContainsKey(Table.MustBones[i]))
                {
                    Print("\t\t丢失骨骼：" + Table.MustBones[i]);
                    MissingBones.Add(Table.MustBones[i]);
                    allRight = false;
                }
            }
            if (allRight)
            {
                Print("\t全部定帧骨骼检查完毕，没有骨骼出现错误");
            }
            else
            {
                Print("\t骨骼检查完毕，存在丢失骨骼，会导致无法输出VMD");
            }
            Result.Add("丢失骨骼", MissingBones);
            Print("==========================");
            Print("正在检查显示枠");
            allRight = true;
            List<string> MissingIK = new List<string>();
            for (int i = 0; i < Table.MustBones.Count; i++)
            {
                if (BoneMap.ContainsKey(Table.MustBones[i]))
                {
                    int BoneID = BoneMap[Table.MustBones[i]];
                    if (!BoneIK.ContainsKey(BoneID))
                    {
                        Print("\t\t丢失显示枠：" + Table.MustBones[i]);
                        MissingIK.Add(Table.MustBones[i]);
                        allRight = false;
                    }
                }
            }
            if (allRight)
            {
                Print("\t全部显示枠检查完毕，没有显示枠出现错误");
            }
            else
            {
                Print("\t显示枠检查完毕，存在丢失显示枠，会导致VMD动作错误");
            }
            Result.Add("丢失显示枠", MissingIK);
            Print("==========================");
            Print("正在检查特殊骨骼");
            allRight = true;
            Print("\t正在左つま先ＩＫ和右つま先ＩＫ");
            List<string> ErrSpecial = new List<string>();
            int LIK = BoneMap.ContainsKey("左つま先ＩＫ") ? BoneMap["左つま先ＩＫ"] : -1;
            int RIK = BoneMap.ContainsKey("右つま先ＩＫ") ? BoneMap["右つま先ＩＫ"] : -1;
            int LNIK = BoneMap.ContainsKey("左つま先") ? BoneMap["左つま先"] : -1;
            int RNIK = BoneMap.ContainsKey("右つま先") ? BoneMap["右つま先"] : -1;
            if (LIK > -1)
            {
                PMDEditor.PmxBone.BoneFlags Flag = PModel.BoneList[LIK].Flags & PMDEditor.PmxBone.BoneFlags.ToBone;
                if (Flag == PMDEditor.PmxBone.BoneFlags.None)
                {
                    ErrSpecial.Add("左つま先ＩＫ");
                    Print("\t\t左つま先ＩＫ 需要矫正");
                    allRight = false;
                }
            }
            if (RIK > -1)
            {
                PMDEditor.PmxBone.BoneFlags Flag = PModel.BoneList[RIK].Flags & PMDEditor.PmxBone.BoneFlags.ToBone;
                if (Flag == PMDEditor.PmxBone.BoneFlags.None)
                {
                    ErrSpecial.Add("右つま先ＩＫ");
                    Print("\t\t右つま先ＩＫ 需要矫正");
                    allRight = false;
                }
            }
            if (LNIK > -1)
            {
            }
            else
            {
                ErrSpecial.Add("MIS右つま先");
                Print("\t\t右つま先 骨骼丢失");
                allRight = false;
            }
            if (RNIK > -1)
            {
            }
            else
            {
                ErrSpecial.Add("MIS左つま先");
                Print("\t\t左つま先 骨骼丢失");
                allRight = false;
            }
            if (allRight)
            {
                Print("\t全部特殊骨骼检查完毕，没有特殊骨骼出现错误");
            }
            else
            {
                Print("\t特殊骨骼检查完毕，存在不可控制的特殊骨骼，会导致VMD脚部错误");
            }
            Result.Add("错误IK", ErrSpecial);
            Print("检查结束！");
            Print("==========================");
            return Result;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            FixErr();
        }
        #endregion

        #region VMDZipper Body
        public void ZipVmd(string SourceFile, string targetFile)
        {
            textBox1.Text="";
            Print("加载VMD文件："+SourceFile);
            LibMMM.VmdData vmd = LibMMM.VmdFile.OpenVmdFile(SourceFile);
            if (vmd != null)
            {
                Print("加载VMD文件成功!");
            }
            else
            {
                Print("加载VMD文件失败!");
            }
            Print("开始压缩数据：");
            Dictionary<string, List<LibMMM.MotionFrameData>> mfdl = vmd.MotionData;
            Dictionary<string, List<LibMMM.MorphFrameData>> mrdl = vmd.MorphData;
            mfdl = RemoveSameMotion(vmd);
            Print("保存VMD文件：" + targetFile);
            SaveVmdFile(targetFile, vmd, mfdl, mrdl);
            Print("保存VMD文件成功!");
        }
        Dictionary<string, List<LibMMM.MotionFrameData>> RemoveSameMotion(LibMMM.VmdData vmd)
        {
            Dictionary<string, List<LibMMM.MotionFrameData>> Ref = new Dictionary<string, List<LibMMM.MotionFrameData>>();
            Dictionary<string, List<LibMMM.MotionFrameData>> ShouldReplace = new Dictionary<string, List<LibMMM.MotionFrameData>>();
            foreach (KeyValuePair<string, List<LibMMM.MotionFrameData>> MV in vmd.MotionData)
            {
                LibMMM.MotionFrameData mfdl = new LibMMM.MotionFrameData();
                int LastIdx = 0;
                List<LibMMM.MotionFrameData> NewValue = new List<LibMMM.MotionFrameData>();
                NewValue.AddRange(MV.Value);
                bool isRemoving = false;
                foreach (LibMMM.MotionFrameData mfd in MV.Value)
                {
                    LastIdx = NewValue.IndexOf(mfd);
                    if (Same(mfd, mfdl))
                    {
                        isRemoving = true;
                        NewValue.Remove(mfd);
                    }
                    else
                    {
                        if (isRemoving)
                        {
                            isRemoving = false;
                            NewValue.Insert(LastIdx, mfdl);
                        }
                    }
                    mfdl = mfd;
                }
                if (NewValue.Count > 0)
                {
                    ShouldReplace.Add(MV.Key, NewValue);
                    Print("\t\t 压缩["+MV.Key+"]骨骼帧从：" + MV.Value.Count + " 到 " + NewValue.Count);
                }
            }
            foreach (KeyValuePair<string, List<LibMMM.MotionFrameData>> MV in vmd.MotionData)
            {
                if (ShouldReplace.ContainsKey(MV.Key))
                {
                    Ref.Add(MV.Key, ShouldReplace[MV.Key]);
                }
                else
                {
                    Ref.Add(MV.Key, MV.Value);
                }
            }
            return Ref;
        }
        bool Same(LibMMM.MotionFrameData la, LibMMM.MotionFrameData lb)
        {
            bool r = true;
            r = r && (la.boneName == lb.boneName);
            r = r && (la.interpolRA == lb.interpolRA);
            r = r && (la.interpolRB == lb.interpolRB);
            r = r && (la.interpolXA == lb.interpolXA);
            r = r && (la.interpolXB == lb.interpolXB);
            r = r && (la.interpolYA == lb.interpolYA);
            r = r && (la.interpolYB == lb.interpolYB);
            r = r && (la.interpolZA == lb.interpolZA);
            r = r && (la.interpolZB == lb.interpolZB);
            r = r && (la.position.X == lb.position.X);
            r = r && (la.position.Y == lb.position.Y);
            r = r && (la.position.Z == lb.position.Z);
            r = r && (la.quaternion.Angle == lb.quaternion.Angle);
            r = r && (la.quaternion.W == lb.quaternion.W);
            r = r && (la.quaternion.X == lb.quaternion.X);
            r = r && (la.quaternion.Y == lb.quaternion.Y);
            r = r && (la.quaternion.Z == lb.quaternion.Z);
            r = r && (la.quaternion.Axis.X == lb.quaternion.Axis.X);
            r = r && (la.quaternion.Axis.Y == lb.quaternion.Axis.Y);
            r = r && (la.quaternion.Axis.Z == lb.quaternion.Axis.Z);
            r = r && (la.StageID == lb.StageID);
            return r;
        }
        void SaveVmdFile(string File, LibMMM.VmdData vmdData, Dictionary<string, List<LibMMM.MotionFrameData>> MFDL, Dictionary<string, List<LibMMM.MorphFrameData>> MRDL)
        {
            List<LibMMM.MorphFrameData> MRD = new List<LibMMM.MorphFrameData>();
            List<LibMMM.MotionFrameData> MFD = new List<LibMMM.MotionFrameData>();
            List<LibMMM.CharacterPropertyData> CPD = new List<LibMMM.CharacterPropertyData>();
            foreach (KeyValuePair<string, List<LibMMM.MorphFrameData>> MV in MRDL)
            {
                MRD.AddRange(MV.Value);
            }
            foreach (KeyValuePair<string, List<LibMMM.MotionFrameData>> MV in MFDL)
            {
                MFD.AddRange(MV.Value);
            }
            LibMMM.VmdFile.SaveVmdFile(File, vmdData.ModelName, 0, vmdData.MaxFrameNumber, MFD, MRD, vmdData.CameraData, vmdData.CameraPropertyData, vmdData.LightData, CPD, vmdData.IkList);

        }
        #endregion

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Width < 1052)
            {
                this.Width = 1052;
            }
        }

        void VSQ2CCS(string SourceVSQ,string TargetCCS)
        {
            LibMMM.VsqFile vf = LibMMM.VsqFile.Open(SourceVSQ);
            int x = 0;
            if (checkBox1.Checked)
            {
                int M = int.MaxValue;
                for (int i = 0; i < vf.Tracks.Count; i++)
                {
                    try
                    {

                        foreach (LibMMM.vsqEvent td in vf.Tracks[i].events)
                        {
                            int a = (int)(td.info.Note / 12) - 2;
                            int b = td.info.Note % 12;
                            if (a < 1) continue;
                            if (td.info.Length < 30) continue;
                            M = Math.Min(M, td.time);
                            break;
                        }
                    }
                    catch { ;}
                }
                if (M < int.MaxValue)
                {
                    x = M;
                }
            }
            string v=CCSBuilder.BuildCCS(vf,x);
            System.IO.File.WriteAllText(TargetCCS, v, Encoding.UTF8);
            if (System.IO.File.Exists(TargetCCS))
            {
                MessageBox.Show("CEVIO工程文件导出到："+TargetCCS+",拖动CCS文件到CharminStudio的嘴型条上即可导入嘴型", "VSQX2CCS");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "打开MMD动作";
            ofd.AddExtension = true;
            ofd.DefaultExt = ".vmd";
            ofd.Filter = "*.vmd|*.vmd";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = "";
                System.IO.FileInfo fi = new FileInfo(ofd.FileName);
                if (fi.Extension.ToLower() != ".vmd")
                {
                    MessageBox.Show("压缩器只处理VMD文件！", "Format Error");
                }
                else
                {
                    string SourceFile = ofd.FileName;
                    string TarFile = ofd.FileName.Substring(0, ofd.FileName.Length - 4) + "_Zipped.vmd";
                    ZipVmd(SourceFile, TarFile);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "打开VSQ文件";
            ofd.AddExtension = true;
            ofd.DefaultExt = ".vsq";
            ofd.Filter = "*.vsq|*.vsq";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = "";
                System.IO.FileInfo fi = new FileInfo(ofd.FileName);
                if (fi.Extension.ToLower() != ".vsq")
                {
                    MessageBox.Show("转换器只处理VSQ文件！", "Format Error");
                }
                else
                {
                    string SourceFile = ofd.FileName;
                    string TarFile = ofd.FileName.Substring(0, ofd.FileName.Length - 4) + "_Mouth.ccs";
                    VSQ2CCS(SourceFile, TarFile);
                }
            }
        }

    }
}

namespace PMXCheckerForOMP
{
    public class Table
    {
        public static List<string> MustBones = new List<string>()
        {
            "センター",
            "上半身", 
            "首",
            "頭",
            "左目",
            "右目",
            "両目",
            "左肩",
            "左腕",
            "左ひじ", 
            "左手首",
            "左親指１", 
            "左親指２", 
            "左人指１",
            "左人指２",
            "左人指３",
            "左中指１",
            "左中指２",
            "左中指３",
            "左薬指１", 
            "左薬指２", 
            "左薬指３", 
            "左小指１", 
            "左小指２",
            "左小指３",
            "右肩",
            "右腕", 
            "右ひじ",
            "右手首",
            "右親指１",
            "右親指２",
            "右人指１",
            "右人指２", 
            "右人指３",
            "右中指１", 
            "右中指２", 
            "右中指３",
            "右薬指１",
            "右薬指２", 
            "右薬指３",
            "右小指１",
            "右小指２",
            "右小指３",
            "下半身", 
            "左足",
            "左ひざ", 
            "左足首",
            "右足", 
            "右ひざ", 
            "右足首"
        };
    }
}
