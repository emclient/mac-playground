using System;
using System.Collections.Generic;
using System.Text;

namespace GUITest
{
	class Contact
	{
		public string Name;
		public string Surname;
		public string Company;
		public List<ContactMail> Mails;

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
			contact.Name = "GUITest_Name";
			contact.Surname = "GUITest_Surname";
			contact.Company = "GUITest_Company";
			contact.Mails = new List<ContactMail>();
			contact.Mails.Add(new ContactMail(ContactMailType.Email, "test_email_personal@example.com"));
			contact.Mails.Add(new ContactMail(ContactMailType.Work, "test_email_work@example.com", "Test Work Email"));
			contact.Mails.Add(new ContactMail(ContactMailType.Home, "test_email_home@example.com"));
			return contact;
		}
	}

	enum ContactMailType
	{
		Email,
		Work,
		Home
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
