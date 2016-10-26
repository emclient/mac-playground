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
		public List<ContactTelephone> Telephones;
		public List<ContactAddress> Addresses;

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
			contact.Telephones = new List<ContactTelephone>();
			contact.Telephones.Add(new ContactTelephone(ContactTelephoneType.Mobile, "777666555"));
			contact.Telephones.Add(new ContactTelephone(ContactTelephoneType.Work, "111222333"));
			contact.Addresses = new List<ContactAddress>();
			contact.Addresses.Add(new ContactAddress(ContactAddressType.Work, "Thámova 18", "Prague", "186 00", "test", "Czech Republic", "50.092409,14.452772"));
			return contact;
		}
	}

	public enum ContactMailType
	{
		Email,
		Work,
		Home
	}

	public enum ContactTelephoneType
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

	public enum ContactAddressType
	{
		Work,
		Home,
		Other,
		Custom
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

	class ContactTelephone
	{
		public ContactTelephoneType Type;
		public string Number;

		public ContactTelephone(ContactTelephoneType type, string number)
		{
			Type = type;
			Number = number;
		}
	}

	class ContactAddress
	{
		public ContactAddressType Type;
		public string Street;
		public string City;
		public string ZIP;
		public string State;
		public string Country;
		public string GPS;

		public ContactAddress(ContactAddressType type, string street, string city, string zip, string state, string country, string gps)
		{
			Type = type;
			Street = street;
			City = city;
			ZIP = zip;
			State = state;
			Country = country;
			GPS = gps;
		}
	}
}
