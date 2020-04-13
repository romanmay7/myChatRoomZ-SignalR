using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ.Data.Models
{
    public class ChatRoomZRepository : IChatRoomZRepository
    {
        private readonly ChatRoomZContext _context;
        private readonly ILogger<ChatRoomZRepository> _logger;
        public  ChatRoomZRepository(ChatRoomZContext context, ILogger<ChatRoomZRepository> logger)
         {
            _context = context;
            _logger = logger;

         }

        public IEnumerable<Channel> GetAllChannels()
        {
            try
            {
                _logger.LogInformation("GetAllChannels");
                //Eager Loading ,including "ChatMessage" Entities
                return _context.Channels.OrderBy(prop => prop.Title).Include("MessageHistory").ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all channels: {ex}");
                return null;
            }

        }

        public bool SaveAll()
        {
            return _context.SaveChanges() > 0;
        }

        public void AddEntity(object model)
        {
            _context.Add(model);
        }

        public void AddMessage(ChatMessage newMsg)
        {
            AddEntity(newMsg);
        }

        private void RemoveEntity(ChatMessage Msg)
        {
            _context.Remove(Msg);
        }
        public void DeleteMessage(ChatMessage model)
        {
            RemoveEntity(model);
        }


    }

}
