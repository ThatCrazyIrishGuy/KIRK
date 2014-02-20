using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIRK
{
    public class Word
    {
        private string keyword;

        public string Keyword
        {
            get { return keyword; }
            set { keyword = value; }
        }

        private bool end;

        public bool End
        {
            get { return end; }
            set { end = value; }
        }

        private bool start;

        public bool Start
        {
            get { return start; }
            set { start = value; }
        }

        public List<string> postwords = new List<string>();

        public List<int> occuranceCount = new List<int>();

        public Word()
        {
            keyword = "";
            End = false;
            Start = false;
            //postwords.Add("");
        }

        public override string ToString()
        {
            string temp = keyword + " " + End + " " + Start;
            for (int i=0; i<postwords.Count; i++) //string s in postwords)
            {
                temp += " " + postwords[i] + "(" + occuranceCount[i] + ")";
            }
            return temp;
        }
    }
}
