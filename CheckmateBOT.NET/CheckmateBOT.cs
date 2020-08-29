using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Linq;

namespace CheckmateBOT.NET
{
    class CheckmateBOT
    {
        /*
         * https://kana.byha.top:444/post/6735
         * https://www.luogu.com.cn/paste/nbyi7ds9
         */

        private string kanaLink;
        private ChromeDriver driver;
        private string username;
        private string password;
        private string roomId;
        private bool isSecret;
        private bool isAutoReady;
        private int[,] mpType;
        private int[,] mpTmp;
        private int[,] mpBelong;
        private bool[,] vis;
        private ArrayList q = new ArrayList();
        public bool error;
        private int sx;
        private int sy;
        private int size;
        private int userCount;
        private Random rd = new Random();

        private int[,] di = { { -1, 0 }, { 0, 1 }, { 1, 0 }, { 0, -1 } };

        public CheckmateBOT(string username, string password, string roomId, bool isSecret = false, bool isAutoReady = true)
        {
            kanaLink = "https://kana.byha.top:444/";
            driver = new ChromeDriver();
            this.username = username;
            this.password = password;
            this.roomId = roomId;
            this.isSecret = isSecret;
            this.isAutoReady = isAutoReady;
            mpType = new int[25, 25];
            mpTmp = new int[25, 25];
            mpBelong = new int[25, 25];
            vis = new bool[25, 25];
            error = false;
            sx = sy = 0;
            size = 20;
        }

        private void SendKeyToTable(string key)
        {
            var ac = new Actions(driver);
            ac.SendKeys(key).Perform();
        }

        // https://blog.csdn.net/weixin_42107267/article/details/93198343
        // 生草方法
        public bool isElementExist(string elementID)
        {
            try
            {
                driver.FindElementById(elementID);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 字面意思
        private void getMap()
        {
            mpType = new int[25, 25];
            mpTmp = new int[25, 25];
            mpBelong = new int[25, 25];
            var s = driver.FindElementById("m").GetAttribute("innerHTML");
            var stype = new ArrayList();
            var stmp = new ArrayList();
            var cnt = 0;
            string g;
            while (true)
            {
                Match tmp = Regex.Match(s, @"class=""[\s\S]*?""");
                if (tmp.Success)
                {
                    g = tmp.Value;
                    g = g.Substring(7, g.Length - 1 - 7);
                    stype.Add(" " + g + " ");
                    int p = s.IndexOf(g);
                    s = s.Substring(p + g.Length, s.Length - (p + g.Length));
                    cnt += 1;
                }
                else
                {
                    break;
                }
                tmp = Regex.Match(s, ">.*?<");
                g = tmp.Value;
                g = g.Substring(1, g.Length - 1 - 1);
                stmp.Add(g);
            }
            size = (int)(Math.Pow(cnt, 0.5));
            if (!(size == 9 || size == 10 || size == 19 || size == 20))
            {
                return;
            }
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    string p = (string)stype[0];
                    stype.RemoveAt(0);
                    if (p.Contains(" unshown "))
                    {
                        mpType[i + 1, j + 1] = -1;
                    }
                    else if (p.Contains(" city "))
                    {
                        mpType[i + 1, j + 1] = 5;
                    }
                    else if (p.Contains(" crown "))
                    {
                        mpType[i + 1, j + 1] = 2;
                    }
                    else if (p.Contains(" mountain "))
                    {
                        mpType[i + 1, j + 1] = 1;
                    }
                    else if (p.Contains(" gas "))
                    {
                        mpType[i + 1, j + 1] = 1;
                    }
                    else if (p.Contains(" null ") && p.Contains(" grey "))
                    {
                        mpType[i + 1, j + 1] = 0;
                    }
                    else if (p.Contains(" null ") && p.Contains(" grey "))
                    {
                        mpType[i + 1, j + 1] = 3;
                    }
                    if (p.Contains(" own "))
                    {
                        mpBelong[i + 1, j + 1] = 1;
                    }
                    else
                    {
                        mpBelong[i + 1, j + 1] = 2;
                    }
                    p = (string)stmp[0];
                    stmp.RemoveAt(0);
                    try
                    {
                        mpTmp[i + 1, j + 1] = int.Parse(p);
                    }
                    catch
                    {
                        mpTmp[i + 1, j + 1] = 0;
                    }
                }
            }
            return;
        }

        // 选择土地
        public void selectLand(int x, int y)
        {
            try
            {
                driver.FindElementById("td-" + ((x - 1) * size + y).ToString()).Click();
                return;
            }
            catch
            {
                return;
            }
        }

        //登录，如果出现异常则在5S后退出
        public void Login()
        {

            Console.WriteLine("正在登录…");
            driver.Url = kanaLink;
            var usernameBox = driver.FindElementByName("username");
            var passwordBox = driver.FindElementByName("pwd");
            var ac = new Actions(driver);

            // 输入账号密码并登录
            ac.SendKeys(usernameBox, username);
            ac.SendKeys(passwordBox, password);
            // 等待用户手动输入验证码
            Thread.Sleep(10000);
            ac.Click(driver.FindElementById("submitButton")).Perform();

            /*
            try:
                WebDriverWait(self.driver, 8).until(EC.url_to_be(self.kanaLink))
                print("登录成功！")
            except TimeoutException:
                print("网络连接出现问题或账密错误！\n程序将在5秒后退出")
                sleep(5)
                self.driver.close()
                del self
             */
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(8));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlToBe(kanaLink));
                Console.WriteLine("登录成功！");
            }
            catch
            {
                Console.WriteLine("网络连接出现问题或账密错误！");
                Thread.Sleep(5000);
                driver.Close();
                //del self 没有del 告辞
            }
        }

        // 进入指定房间
        public void EnterRoom()
        {
            driver.Url = "https://kana.byha.top:444/checkmate/room/" + roomId;

            if (isSecret)
            {
                var settingBtn = driver.FindElementByClassName("form-check-input");
                var ac = new Actions(driver);
                ac.Click(settingBtn).Perform();
            }
            Console.WriteLine("Bot已就位！");
        }

        // 准备开始，如果300秒未开始，程序退出
        public void Ready()
        {
            try
            {
                userCount = int.Parse(driver.FindElementById("total-user").Text);
            }
            catch
            {
                userCount = 3;
            }
            var ac = new Actions(driver);
            ac.Click(driver.FindElementById("ready")).Perform();

            try
            {
                /*
                WebDriverWait(self.driver, 300).until(
                    EC.visibility_of_element_located((By.TAG_NAME, "tbody")))
                */
                //操你妈的傻逼Selenium 一堆函数都他妈不一样 老子找半天命名空间发现没有 操你妈的
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
                //鬼知道这里会不会出傻逼问题 傻逼selenium 操你妈的 反正应该不会有两个tbody
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(driver.FindElementsByTagName("tbody")));
            }
            catch
            {
                Console.WriteLine("房间内无人开始，过一会再试试吧");
                Thread.Sleep(5000);
                Kill();
            }
        }

        private void Kill()
        {
            driver.Close();
            //del self 我tm del你妈
        }

        private void Pr(string c)
        {
            SendKeyToTable(c);
            // print(c)
        }

        private bool isOutside(int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {

                int px = x + di[i, 0];
                int py = y + di[i, 1];
                if (px >= 1 && px <= size && py >= 1 && py <= size && mpBelong[px, py] == 2)
                {
                    return true;
                }
            }
            return false;
        }

        private void changeTarget()
        {
            var insideAnsTmp = mpTmp[sx, sy];
            var insideAnsX = sx;
            var insideAnsY = sy;
            var outsideAnsTmp = 0;
            var outsideAnsX = 0;
            var outsideAnsY = 0;
            for (int p = 0; p < size; p++)
            {
                for (int q = 0; q < size; q++)
                {
                    var i = p + 1;
                    var j = q + 1;
                    if (mpBelong[i, j] == 1)
                    {
                        if (isOutside(i, j))
                        {
                            if (mpTmp[i, j] > outsideAnsTmp)
                            {
                                outsideAnsTmp = mpTmp[i, j];
                                outsideAnsX = i;
                                outsideAnsY = j;
                            }
                        }
                        else
                        {
                            if (mpTmp[i, j] > insideAnsTmp)
                            {
                                insideAnsTmp = mpTmp[i, j];
                                insideAnsX = i;
                                insideAnsY = j;
                            }
                        }
                    }
                }
            }
            if (outsideAnsTmp * 5 >= insideAnsTmp)
            {
                sx = outsideAnsX;
                sy = outsideAnsY;
            }
            else
            {
                sx = insideAnsX;
                sy = insideAnsY;
            }
            q.Add(new int[2] { sx, sy });
            if (rd.Next(0, 2) == 1)
            {
                vis = new bool[25, 25];
            }
            vis[sx, sy] = true;
            selectLand(sx, sy);
            return;
        }

        private void botMove()
        {
            Thread.Sleep(250);
            var x = 0;
            var y = 0;
            var tryTime = 0;
            getMap();
            while (true)
            {
                if (q.Count == 0)
                {
                    changeTarget();
                }
                // 妈的真难看
                x = ((int[])(q[0]))[0];
                y = ((int[])(q[0]))[1];
                tryTime += 1;
                q.RemoveAt(0);
                if (!(mpTmp[x, y] <= 1 && mpType[x, y] != 2 && tryTime <= 10))
                {
                    break;
                }
            }
            if (tryTime > 10)
            {
                return;
            }
            if (mpTmp[x,y] <= 1)
            {
                return;
            }
            if (mpBelong[x, y] == 2)
            {
                return;
            }
            var ansTmp = 0;
            var ansI = -1;
            int[] tmpI = [0, 1, 2, 3];
            // random.shuffle(tmpI)
            tmpI = tmpI.OrderBy(c => Guid.NewGuid()).ToArray<int>();

            foreach (var i in tmpI)
            {
                var px = x + di[i, 0];
                var py = y + di[i, 1];
                if (px >= 1 && px <= size && py >= 1 && py <= size && mpType[px, py] != 1 && (!vis[px, py]) && (mpType[px, py] != 5 || mpTmp[x, y] > mpTmp[px, py]))
                {
                    var currentTmp = 0;
                    if (mpBelong[px, py] == 2)
                    {
                        if (mpType[px, py] == 2)
                        {
                            currentTmp = 10;
                        }
                        else if (mpType[px, py] == 5)
                        {
                            currentTmp = 8;
                        }
                        else if (mpType[px][py] == 3)
                        {
                            currentTmp = 5;
                        }
                        else
                        {
                            currentTmp = 3;
                        }
                    }
                    else
                    {
                        currentTmp = 1;
                    }
                    if (currentTmp > ansTmp)
                    {
                        ansTmp = currentTmp;
                        ansI = i;
                    }
                }
            }
            if (ansI == -1)
            {
                return;
            }
            px = x + self.di[ansI][0]
        py = y + self.di[ansI][1]
        self.vis[px][py] = True
        self.q.append([px, py])
        if ansI == 0:
            self.Pr('W')
        elif ansI == 1:
            self.Pr('D')
        elif ansI == 2:
            self.Pr('S')
        else:
            self.Pr('A')
        self.botMove()
        return
                }
    }
}
/*
class Bot(object):


    def Main(self):
        self.Login()
        self.EnterRoom()
        self.table = self.driver.find_element_by_tag_name("tbody")
        while True:
            if self.isAutoReady:
                self.Ready()
            self.Pr('F') // 防踢
            self.getMap()
            self.sx = 0
            self.sy = 0
            for i in range(self.size):
                for j in range(self.size):
                    if self.mpBelong[i + 1][j + 1] == 1 and self.mpType[i + 1][j + 1] == 2:
                        self.sx = i + 1
                        self.sy = j + 1
            if self.sx == 0 or self.sy == 0:
                continue
            self.changeTarget()
            self.botMove()
        return


print("输入用户名：")
t1 = input()
print("输入密码：")
t2 = input()
print("输入房间号：")
t3 = input()
a = Bot(t1, t2, t3)
a.Main()

*/