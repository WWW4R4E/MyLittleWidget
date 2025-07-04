using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Models
{
    public class Contact
    {
        #region Properties
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Company { get; private set; }
        public string Name => FirstName + " " + LastName;
        #endregion

        public Contact(string firstName, string lastName, string company)
        {
            FirstName = firstName;
            LastName = lastName;
            Company = company;
        }

        #region Public Methods
        public async static Task<ObservableCollection<Contact>> GetContactsAsync()
        {
            IList<string> lines = new List<string>
{
    "张", "三", "微软",
    "李", "四", "阿里巴巴",
    "王", "五", "腾讯",
    "赵", "六", "百度",
    "孙", "七", "字节跳动"
};

            ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();

            for (int i = 0; i < lines.Count - 2; i += 3)
            {
                contacts.Add(new Contact(lines[i], lines[i + 1], lines[i + 2]));
            }

            return contacts;
        }

        //public static async Task<ObservableCollection<GroupInfoList>> GetContactsGroupedAsync()
        //{
        //    var query = from item in await GetContactsAsync()
        //                group item by item.LastName.Substring(0, 1).ToUpper() into g
        //                orderby g.Key
        //                select new GroupInfoList(g) { Key = g.Key };

        //    return new ObservableCollection<GroupInfoList>(query);
        //}

        public override string ToString()
        {
            return $"{Name}, {Company}";
        }
        #endregion
    }
}
