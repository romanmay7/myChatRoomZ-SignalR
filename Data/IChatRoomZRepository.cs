using myChatRoomZ.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ.Data
{
    public interface IChatRoomZRepository
    {
        public IEnumerable<Channel> GetAllChannels();
        void AddEntity(object model);
        void AddMessage(ChatMessage model);
        bool SaveAll();
    }
}
