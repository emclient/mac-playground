using System;
using System.Text;

namespace System.Windows.Forms
{
	internal static class HtmlClip
	{
		const string Header = "Version:0.9\n\r\nStartHTML:<<<<<<<<1\r\nEndHTML:<<<<<<<<2\r\nStartFragment:<<<<<<<<3\r\nEndFragment:<<<<<<<<4\r\nStartSelection:<<<<<<<<3\r\nEndSelection:<<<<<<<<4";
		const string StartFragment = "<!--StartFragment-->";
		const string EndFragment = @"<!--EndFragment-->";

		public static string AddMetadata(string html)
		{
			var sb = new StringBuilder();
			sb.AppendLine(Header);
			sb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

			const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
			// if given html already provided the fragments we won't add them
			int fragmentStart, fragmentEnd;
			int fragmentStartIdx = html.IndexOf(StartFragment, comparison);
			int fragmentEndIdx = html.LastIndexOf(EndFragment, comparison);

			// if html tag is missing add it surrounding the given html (critical)
			int htmlOpenIdx = html.IndexOf("<html", comparison);
			int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
			int htmlCloseIdx = html.LastIndexOf("</html", comparison);

			if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
			{
				int bodyOpenIdx = html.IndexOf("<body", comparison);
				int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

				if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
				{
					// the given html doesn't contain html or body tags so we need to add them and place start/end fragments around the given html only
					sb.Append("<html><body>");
					sb.Append(StartFragment);
					fragmentStart = GetByteCount(sb);
					sb.Append(html);
					fragmentEnd = GetByteCount(sb);
					sb.Append(EndFragment);
					sb.Append("</body></html>");
				}
				else
				{
					// insert start/end fragments in the proper place (related to html/body tags if exists) so the paste will work correctly
					int bodyCloseIdx = html.LastIndexOf("</body", comparison);

					if (htmlOpenEndIdx < 0)
						sb.Append("<html>");
					else
						sb.Append(html, 0, htmlOpenEndIdx);

					if (bodyOpenEndIdx > -1)
						sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));

					sb.Append(StartFragment);
					fragmentStart = GetByteCount(sb);

					var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
					var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
					sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

					fragmentEnd = GetByteCount(sb);
					sb.Append(EndFragment);

					if (innerHtmlEnd < html.Length)
						sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

					if (htmlCloseIdx < 0)
						sb.Append("</html>");
				}
			}
			else
			{
				// handle html with existing start\end fragments just need to calculate the correct bytes offset (surround with html tag if missing)
				if (htmlOpenEndIdx < 0)
					sb.Append("<html>");
				int start = GetByteCount(sb);
				sb.Append(html);
				fragmentStart = start + GetByteCount(sb, start, start + fragmentStartIdx) + StartFragment.Length;
				fragmentEnd = start + GetByteCount(sb, start, start + fragmentEndIdx);
				if (htmlCloseIdx < 0)
					sb.Append("</html>");
			}

			// Back-patch offsets (scan only the header part for performance)
			sb.Replace("<<<<<<<<1", Header.Length.ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, Header.Length);
			sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, Header.Length);

			return sb.ToString();
		}

		public static string RemoveHeader(string s)
		{
			int index;
			var comparison = StringComparison.InvariantCultureIgnoreCase;
			if ((index = s.IndexOf("Version:", comparison)) != 0)
				return s;

			if ((index = s.IndexOf("<!DOCTYPE", comparison)) >= 0)
				return s.Substring(index);

			if ((index = s.IndexOf("<html", comparison)) >= 0)
				return s.Substring(index);

			if ((index = s.IndexOf("<", comparison)) >= 0)
				return s.Substring(index);

			return s;
		}

		static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
		{
			int count = 0;
			char[] byteCount = new char[1];
			end = end > -1 ? end : sb.Length;
			for (int i = start; i < end; i++)
			{
				byteCount[0] = sb[i];
				count += Encoding.UTF8.GetByteCount(byteCount);
			}
			return count;
		}

	}
}
