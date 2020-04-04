using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myChatRoomZ.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace myChatRoomZ.Data
{
    public class ChatRoomZContext : DbContext
    {
        public ChatRoomZContext(DbContextOptions<ChatRoomZContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Channel> Channels { get; set; }
        public new DbSet<ChatUser> ChatUsers { get; set; }
    }
}

