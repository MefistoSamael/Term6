using System.Text.RegularExpressions;
namespace Lab6
{
    public partial class Form1 : Form
    {
        private enum R
        {
            A,
            U,
            G
        }

        private class D
        {
            public bool P { get; set; }
            public bool B { get; set; }
            public bool X { get; set; }
            public bool Ds { get; set; }
        }

        private class U
        {
            public string L { get; set; }
            public string Pw { get; set; }
            public R Rl { get; set; }
            public bool I { get; set; }
            public int Si { get; set; } = -1;

            public U(string l, string pw)
            {
                L = l;
                Pw = pw;
            }

            public override bool Equals(object obj)
            {
                return obj is U u && L == u.L;
            }
        }

        private List<U> ud = new();

        private int l = 2;

        public Form1()
        {
            I();

            IU();
        }

        private void I()
        {
            ud.Add(new U("a", "1"));
            ud.Add(new U("u", "1"));
            ud.Add(new U("g", "1"));

            ud[0].Rl = R.A;
            ud[1].Rl = R.U;
            ud[2].Rl = R.G;
        }

        private void IU()
        {
            for (int i = 1; i <= ud.Count; i++)
            {
                messageFields.Add(this.Controls[$"M{i}"] as TextBox);
                signOutButtons.Add(this.Controls[$"SO{i}"] as Button);
                sendButtons.Add(this.Controls[$"S{i}"] as Button);
                usernameLabels.Add(this.Controls[$"UN{i}"] as Label);
            }
        }

        private int GI(Control s) => int.Parse(s.Name.Last().ToString()) - 1;

        private int GLI()
        {
            return int.Parse(messageFields.Where(m => m.Visible == false).First().Name.Last().ToString()) - 1;
        }


        private bool IAL() => ud.Count(user => user.I) >= l;
        private bool ISAL() => ud.Count(user => user.I) == l - 1;

        private D GDS()
        {
            return new D
            {
                P = AttackDefensesCheckedListBox.CheckedIndices.Contains(0),
                B = AttackDefensesCheckedListBox.CheckedIndices.Contains(1),
                Ds = AttackDefensesCheckedListBox.CheckedIndices.Contains(2),
                X = AttackDefensesCheckedListBox.CheckedIndices.Contains(3)
            };
        }

        // S_C, S_C, S_C methods remain the same, with one change:
        // Replace IAL with IAL

        private void S_C(object sender, EventArgs e)
        {
            int i = GLI();
            string l = Login1.Text;
            string p = Password1.Text;
            U ub = ud.Find(u => u.L == l);
            D d = GDS();


            if (i < 0 || i >= ud.Count)
            {
                return;
            }

            if (ISAL() && !d.Ds)
            {
                MessageBox.Show("ALARM!!!!! The server load is high. Next connection will shut down the server");
            }

            if (IAL())
            {
                if (d.Ds)
                {
                    MessageBox.Show("The server is full. Wait your turn");
                    return;
                }
                else
                {
                    MessageBox.Show("The server load is too high. Shutting down");
                    Application.Exit();
                }
            }



            if (ub is null)
            {
                MessageBox.Show($"User {l} doesn't exists", "Authorization error");
                return;
            }


            if (ub is not null && ub.I)
            {
                MessageBox.Show($"User {l} already authorized in {ub.Si + 1} slot", "Authorization error");
                return;
            }


            if (ub != null && ub.Pw == p)
            {
                ub.I = true;
                ub.Si = i;

                messageFields[i].Visible = true;
                signOutButtons[i].Visible = true;
                sendButtons[i].Visible = true;
                usernameLabels[i].Text = "Username: " + ub.L;
            }
        }

        private void Se_C(object sender, EventArgs e)
        {
            int i = GI(sender as Button);
            string m = messageFields[i].Text;
            string a;
            var ubi = ud.Find(u => u.Si == i);
            D d = GDS();

            if (0 > i || i >= ud.Count)
            {
                return;
            }

            if (ubi == null)
            {
                MessageBox.Show("The user must be authorized to send messages", "User Error");
                return;
            }

            if (d.X)
            {
                if (!Regex.IsMatch(m, @"^[A-z0-9]*$"))
                {
                    MessageBox.Show("Invalid message. Please use only English letters and numbers", "Message Error");
                    return;
                }
            }

            if (d.P && ubi.Rl == R.G)
            {
                MessageBox.Show("Insufficient privileges!", "Access Error");
                return;
            }

            if (d.B)
            {
                int s = 10;
                char[] b = new char[s];

                try
                {
                    m.CopyTo(0, b, 0, m.Length);
                    m = string.Join("", b);

                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("The message exceeded the buffer. Part of the data was written to adjacent memory", "Buffer Overflow");
                }

                m = string.Join("", b);
            }

            m = m.Insert(0, $"[{ubi.Rl}] {ubi.L}: ");

            MessagesListBox.Items.Add(m);
        }

        private void SO_C(object sender, EventArgs e)
        {
            int i = GI(sender as Button);
            var ubi = ud.Find(u => u.Si == i);


            if (ubi == null)
            {
                MessageBox.Show("This slot is empty because the user is not authorized", "Sign Out Error");
                return;
            }

            if (ubi != null)
            {
                ubi.Si = -1;
                ubi.I = false;

                messageFields[i].Visible = false;
                signOutButtons[i].Visible = false;
                sendButtons[i].Visible = false;
                usernameLabels[i].Text = "";
            }
        }
    }

}
