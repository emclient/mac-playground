using System;
using System.Collections.Generic;
using System.Text;

using MailClient.Contact;

namespace GUITest
{
	class Contact
	{
		public ContactName Name;
		public string Company;
		public string Category;
		public List<ContactMail> Mails;
		public List<string> Telephones;

		private static object oLockInstance = new object();
		private static Contact testContact;
		public static Contact TestContact
		{
			get
			{
				if (testContact == null)
				{
					lock (oLockInstance)
					{
						if (testContact == null)
						{
							testContact = CreateNewTestContact();
						}
					}
				}
				return testContact;
			}
		}

		public static Contact CreateNewTestContact()
		{
			Contact contact = new Contact();
			ContactName.TryParse("GUITest_Name GUITest_Surname", out contact.Name);
			contact.Company = "GUITest_Company";
			contact.Category = "Home";
			contact.Mails = new List<ContactMail>();
			contact.Mails.Add(new ContactMail(ContactMailType.Email, "test_email_personal@example.com"));
			contact.Mails.Add(new ContactMail(ContactMailType.Work, "test_email_work@example.com", "Test Work Email"));
			contact.Mails.Add(new ContactMail(ContactMailType.Home, "test_email_home@example.com"));
			contact.Telephones = new List<string>();
			contact.Telephones.Add("111222333");
			return contact;
		}
	}

	public enum ContactMailType
	{
		Email,
		Work,
		Home
	}

	public enum ContactPhoneType
	{
		Work,
		Fax,
		Car,
		Company,
		Home,
		FaxHome,
		Mobile,
		Other,
		OtherFax,
		Pager,
		ISDN
	}

	class ContactMail
	{
		public ContactMailType Type;
		public string Address;
		public string DisplayAs;

		public ContactMail(ContactMailType type, string address, string displayAs = null)
		{
			Type = type;
			Address = address;
			DisplayAs = displayAs;
		}
	}
}
