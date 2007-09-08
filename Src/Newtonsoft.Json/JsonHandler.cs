using System;
using System.Web;
using System.Web.Caching;
using System.Xml;
using System.Text;
using System.Security.Principal;

namespace Newtonsoft.Json
{
	public abstract class JsonHandler : HandlerBase, IHttpHandler
	{
		public event EventHandler Error;

		protected abstract void WriteResult(JsonWriter writer);

		public static void JsonResponse(HttpResponse response, Action<JsonWriter> writeAction)
		{
			response.ClearHeaders();
			response.ClearContent();

			JsonWriter writer = new JsonWriter(response.Output);

			writeAction(writer);

			writer.Flush();
		}

		protected virtual void OnError(EventArgs e)
		{
			if (Error != null)
			{
				Error(this, e);
			}
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			Context = context;

			try
			{
				JsonHandler.JsonResponse(context.Response, new Action<JsonWriter>(WriteResult));
			}
			catch (Exception exception)
			{
				context.AddError(exception);
				OnError(EventArgs.Empty);
				if (context.Error != null)
				{
					throw new HttpUnhandledException("blah", exception);
				}
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return false; }
		}
	}
}