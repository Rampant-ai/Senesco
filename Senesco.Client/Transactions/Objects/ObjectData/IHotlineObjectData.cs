
namespace Senesco.Client.Transactions.Objects.ObjectData
{
   /// <summary>
   /// Interface and base type for all low-level Hotline Object Data component
   /// objects, which are essentially platform-independent primitives.
   /// </summary>
   public interface IHotlineObjectData
   {
      byte[] GetBytes();
   }
}
