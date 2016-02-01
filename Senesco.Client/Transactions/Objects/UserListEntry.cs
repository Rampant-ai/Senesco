
using Senesco.Client.Transactions.Objects.ObjectData;
using Senesco.Client.Utility;

namespace Senesco.Client.Transactions.Objects
{
   public class UserListEntry : HotlineObject
   {
      public Short Socket;
      public Short Icon;
      public Short Status;
      public Short NickLength;
      public NormalString Nick;

      /// <summary>
      /// Default creator for the ObjectFactory to use.
      /// </summary>
      public UserListEntry()
      { }

      public UserListEntry(int socket, int icon, int status, string nick)
      {
         Socket = new Short(socket);
         Icon = new Short(icon);
         Status = new Short(status);
         NickLength = new Short(nick.Length);
         Nick = new NormalString(nick);

         this.ObjectDataList.Add(Socket);
         this.ObjectDataList.Add(Icon);
         this.ObjectDataList.Add(Status);
         this.ObjectDataList.Add(NickLength);
         this.ObjectDataList.Add(Nick);
      }

      internal override void ParseBytes(byte[] objectData)
      {
         int index = 0;

         int socket = DataUtils.ReadShort(objectData, ref index);
         int icon = DataUtils.ReadShort(objectData, ref index);
         int status = DataUtils.ReadShort(objectData, ref index);
         int nickLength = DataUtils.ReadShort(objectData, ref index);
         string nick = DataUtils.ReadString(objectData, ref index, nickLength);

         Socket = new Short(socket);
         Icon = new Short(icon);
         Status = new Short(status);
         NickLength = new Short(nickLength);
         Nick = new NormalString(nick);

         this.ObjectDataList.Add(Socket);
         this.ObjectDataList.Add(Icon);
         this.ObjectDataList.Add(Status);
         this.ObjectDataList.Add(NickLength);
         this.ObjectDataList.Add(Nick);
      }
   }
}
