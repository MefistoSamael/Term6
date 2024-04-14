using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.ApplicationServices;
//data structures
namespace Lab6
{
    public partial class Form2 : Form
    {
        private int Const2 = 2;

        private enum Roles
        {
            Admin,
            Guest,
            User,
            SuperUser,
            Accounter
        }

        private class DefenseSystemSwitcher
        {
            public bool PrivilegeMinimization { get; set; }
            public bool Buffer { get; set; }
            public bool Xss { get; set; }
            public bool Dos { get; set; }
        }

        public class P1
        {
            static public int ch = 39;
            public string Username { get; set; }
            public int Role { get; set; }
            public bool IsSignedIn { get; set; }
        }

        public class P2
        {
            public string Password { get; set; }

            private int slotIndex = -1;
            public int SlotIndex
            {
                get
                {
                    var cst = Eval(Const1, 4);
                    var a = cst ^ 7;
                    return slotIndex + ((a | Const1 / 6) & ~(a & Const1 / 7 + 1));
                }
                set
                {
                    slotIndex = value - Eval();
                }
            }

            private int Const1 = 42;

            private int Eval(int a = 3, int c = 4)
            {
                Random aa = new Random();

                var b = (aa.Next(c) + a / 3 * 52) % 4;

                int z = Convert.ToInt32((b + 42 * Math.Pow(10, b)) / Math.PI * (70 - (Const1 + 28)));

                return (int)(P1.ch + z + 52);
            }
        }

        private class User
        {
            private P1 a = new();
            private P2 b = new();
            public string Login { get => a.Username; set => a.Username = value; }
            public string Password { get => b.Password; set => b.Password = value; }
            public int Role { get => a.Role; set => a.Role = value; }
            public bool IsSignedIn { get => a.IsSignedIn; set => a.IsSignedIn = value; }
            public int SlotIndex { get => b.SlotIndex; set => b.SlotIndex = value; }

            public User(string login, string password)
            {
                Login = login;
                Password = password;
            }

            public override bool Equals(object obj)
            {
                return obj is User user && Login == user.Login;
            }
        }

        private List<User> usersData = new();

        private int limit = 2;

        public Form2()
        {
            InitializeComponent();

            InitializeUsersData();
            AssignControlsToUsers();
        }

        private void InitializeUsersData()
        {
            usersData.Add(new User("a", "1"));
            usersData.Add(new User("u", "1"));
            usersData.Add(new User("g", "1"));

            usersData[0].Role = 0;
            usersData[1].Role = 1;
            usersData[2].Role = 2;
        }

        private void AssignControlsToUsers()
        {
            for (int i = 1; i <= usersData.Count; i++)
            {
                messageFields.Add(this.Controls[$"Message{i}"] as TextBox);
                signOutButtons.Add(this.Controls[$"SignOut{i}"] as Button);
                sendButtons.Add(this.Controls[$"Send{i}"] as Button);
                usernameLabels.Add(this.Controls[$"Username{i}"] as Label);
            }
        }

        private int GetIndex(Control sender) => int.Parse(sender.Name.Last().ToString()) - 1;

        private int GetLogInIndex()
        {
            return int.Parse(messageFields.Where(m => m.Visible == false).First().Name.Last().ToString()) - 1;
        }


        private bool IsServerLagging() => usersData.Count(user => user.IsSignedIn) >= limit;
        private bool IsServerAlmostLagging() => usersData.Count(user => user.IsSignedIn) == limit - 1;

        private DefenseSystemSwitcher GetDefenseSystemSwitcher()
        {
            return new DefenseSystemSwitcher
            {
                PrivilegeMinimization = AttackDefensesCheckedListBox.CheckedIndices.Contains(0),
                Buffer = AttackDefensesCheckedListBox.CheckedIndices.Contains(1),
                Dos = AttackDefensesCheckedListBox.CheckedIndices.Contains(2),
                Xss = AttackDefensesCheckedListBox.CheckedIndices.Contains(3)
            };
        }

        private void SignIn_Click(object sender, EventArgs e)
        {
            int index = GetLogInIndex();
            string login = Login1.Text;
            string password = Password1.Text;
            User userByName = usersData.Find(user => user.Login == login);
            DefenseSystemSwitcher defense = GetDefenseSystemSwitcher();


            if (index < 0 || index >= usersData.Count)
            {
                return;
            }

            if (IsServerAlmostLagging() && !defense.Dos)
            {
                MessageBox.Show("ALARM!!!!! The server load is high. Next connection will shut down the server");
            }

            if (IsServerLagging())
            {
                if (defense.Dos)
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

            

            if (userByName is null)
            {
                MessageBox.Show($"User {login} doesn't exists", "Authorization error");
                return;
            }


            if (userByName is not null && userByName.IsSignedIn)
            {
                MessageBox.Show($"User {login} already authorized in {userByName.SlotIndex + 1} slot", "Authorization error");
                return;
            }


            if (userByName != null && userByName.Password == password)
            {
                userByName.IsSignedIn = true;
                userByName.SlotIndex = index;

                messageFields[index].Visible = true;
                signOutButtons[index].Visible = true;
                sendButtons[index].Visible = true;
                usernameLabels[index].Text = "Username: " + userByName.Login;
            }
        }

        private void Se_C(object sender, EventArgs e)
        {
            int index = GetIndex(sender as Button);
            string messageText = messageFields[index].Text;
            string answer;
            var userByIndex = usersData.Find(user => user.SlotIndex == index);
            DefenseSystemSwitcher defense = GetDefenseSystemSwitcher();

            if (0 > index || index >= usersData.Count)
            {
                return;
            }

            // If the user at the specified index is not found (null), display an error message.
            if (userByIndex == null)
            {
                MessageBox.Show("The user must be authorized to send messages", "User Error");
                return;
            }

            // If Cross-Site Scripting (XSS) defenses are active, check the message text for invalid characters.
            if (defense.Xss)
            {
                // If the message text contains characters other than English letters and numbers, display an error message.
                if (!Regex.IsMatch(messageText, @"^[A-z0-9]*$"))
                {
                    MessageBox.Show("Invalid message. Please use only English letters and numbers", "Message Error");
                    return;
                }
            }

            // If Privilege Minimization defense is active and the user's role is Client,
            // they do not have sufficient privileges to proceed.
            if (defense.PrivilegeMinimization && userByIndex.Role == 2)
            {
                MessageBox.Show("Insufficient privileges!", "Access Error");
                return;
            }

            if (defense.Buffer)
            {
                int size = 10;
                char[] buf = new char[size];

                try
                {
                    messageText.CopyTo(0, buf, 0, messageText.Length);
                    messageText = string.Join("", buf);

                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("The message exceeded the buffer. Part of the data was written to adjacent memory", "Buffer Overflow");
                }

                messageText = string.Join("", buf);
            }

            messageText = messageText.Insert(0, $"[{userByIndex.Role}] {userByIndex.Login}: ");

            MessagesListBox.Items.Add(messageText);
        }

        private void SignOut_Click(object sender, EventArgs e)
        {
            int index = GetIndex(sender as Button);
            var userByIndex = usersData.Find(user => user.SlotIndex == index);


            if (userByIndex == null)
            {
                MessageBox.Show("This slot is empty because the user is not authorized", "Sign Out Error");
                return;
            }

            if (userByIndex != null)
            {
                userByIndex.SlotIndex = -1;
                userByIndex.IsSignedIn = false;

                messageFields[index].Visible = false;
                signOutButtons[index].Visible = false;
                sendButtons[index].Visible = false;
                usernameLabels[index].Text = "";
            }
        }
    }
}