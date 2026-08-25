using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMember> ClubMembers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<PostShare> PostShares { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendance> EventAttendances { get; set; }
        public DbSet<EventJoinRequest> EventJoinRequests { get; set; }
        public DbSet<FileResource> FileResources { get; set; }
        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<ProfileView> ProfileViews { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }
        public DbSet<ClubApplication> ClubApplications { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryView> StoryViews { get; set; }
        public DbSet<ClubRecommendationDismissal> ClubRecommendationDismissals { get; set; }
        public DbSet<LiveParticipant> LiveParticipants { get; set; }
        public DbSet<LiveChatMessage> LiveChatMessages { get; set; }
        public DbSet<LiveModeration> LiveModerations { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<ClubInvite> ClubInvites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasQueryFilter(x => !x.IsDeleted);


            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Club)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Post>().HasIndex(x => x.UserId);
            modelBuilder.Entity<Post>().HasIndex(x => x.ClubId);


            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reaction>().HasIndex(x => new { x.UserId, x.PostId }).IsUnique();
            modelBuilder.Entity<Reaction>().HasIndex(x => x.PostId);
            modelBuilder.Entity<Reaction>().HasIndex(x => new { x.PostId, x.Type });


            modelBuilder.Entity<Club>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMember>().HasIndex(x => new { x.UserId, x.ClubId }).IsUnique();


            modelBuilder.Entity<Follow>().HasIndex(x => new { x.FollowerId, x.FollowingId }).IsUnique();

            modelBuilder.Entity<Follow>()
                .HasOne(x => x.Follower)
                .WithMany()
                .HasForeignKey(x => x.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(x => x.Following)
                .WithMany()
                .HasForeignKey(x => x.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<BlockedUser>().HasIndex(x => new { x.BlockerId, x.BlockedUserId }).IsUnique();

            modelBuilder.Entity<BlockedUser>()
                .HasOne(x => x.Blocker)
                .WithMany()
                .HasForeignKey(x => x.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlockedUser>()
                .HasOne(x => x.BlockedUserInfo)
                .WithMany()
                .HasForeignKey(x => x.BlockedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasIndex(x => new { x.SenderId, x.ReceiverID });
            modelBuilder.Entity<Message>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Message>().HasIndex(x => new { x.ReceiverID, x.IsSeen });

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Group>()
                .HasOne(x => x.Creator)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasIndex(x => x.CreatedBy);

            modelBuilder.Entity<GroupMember>().HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
            modelBuilder.Entity<GroupMember>().HasIndex(x => x.UserId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(x => x.Group)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMessage>().HasIndex(x => x.GroupId);
            modelBuilder.Entity<GroupMessage>().HasIndex(x => x.SenderId);
            modelBuilder.Entity<GroupMessage>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<GroupMessage>().HasIndex(x => new { x.GroupId, x.CreatedAt });

            modelBuilder.Entity<GroupMessage>()
                .HasOne(x => x.Group)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMessage>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Notification>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>().HasIndex(x => x.ReceiverId);
            modelBuilder.Entity<Notification>().HasIndex(x => x.Type);
            modelBuilder.Entity<Notification>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Notification>().HasIndex(x => new { x.ReceiverId, x.IsRead });


            modelBuilder.Entity<Event>().HasIndex(x => x.EventDate);
            modelBuilder.Entity<Event>().HasIndex(x => x.ClubId);
            modelBuilder.Entity<Event>().HasIndex(x => x.CreatedBy);

            modelBuilder.Entity<EventAttendance>().HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

            modelBuilder.Entity<EventAttendance>()
                .HasOne(x => x.Event)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventAttendance>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<EventJoinRequest>()
                .HasIndex(x => new { x.EventId, x.UserId })
                .IsUnique();

            modelBuilder.Entity<EventJoinRequest>()
                .HasIndex(x => new { x.EventId, x.Status });

            modelBuilder.Entity<EventJoinRequest>()
                .HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventJoinRequest>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<FileResource>()
                .HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<FileResource>().HasIndex(x => x.UploadedBy);
            modelBuilder.Entity<FileResource>().HasIndex(x => x.UploadedAt);
            modelBuilder.Entity<FileResource>().HasIndex(x => x.ClubId);
            modelBuilder.Entity<FileResource>().HasIndex(x => x.FileType);

            modelBuilder.Entity<FileResource>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UploadedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FileResource>()
                .HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<SavedPost>().HasIndex(x => new { x.UserId, x.PostId }).IsUnique();
            modelBuilder.Entity<SavedPost>().HasIndex(x => x.SavedAt);

            modelBuilder.Entity<SavedPost>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedPost>()
                .HasOne(x => x.Post)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<PostReport>().HasIndex(x => new { x.ReporterId, x.PostId }).IsUnique();

            modelBuilder.Entity<PostReport>()
                .HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostReport>()
                .HasOne(x => x.Post)
                .WithMany()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CommentReaction>().HasIndex(x => new { x.CommentId, x.UserId }).IsUnique();

            modelBuilder.Entity<CommentReaction>()
                .HasOne(x => x.Comment)
                .WithMany()
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentReaction>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubApplication>().HasIndex(x => new { x.ClubId, x.Status });
            modelBuilder.Entity<ClubApplication>().HasIndex(x => new { x.UserId, x.Status });
            modelBuilder.Entity<ClubApplication>().HasIndex(x => x.AppliedAt);

            modelBuilder.Entity<ClubApplication>()
                .HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubApplication>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubApplication>()
                .HasOne(x => x.Reviewer)
                .WithMany()
                .HasForeignKey(x => x.ReviewedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubApplication>()
            .HasIndex(x => new { x.UserId, x.ClubId })
            .HasFilter($"[{nameof(ClubApplication.Status)}] = 0") // 0 = Pending
            .IsUnique();


            modelBuilder.Entity<Poll>().HasIndex(x => x.ClubId);
            modelBuilder.Entity<Poll>().HasIndex(x => x.EndDate);
            modelBuilder.Entity<Poll>().HasIndex(x => new { x.ClubId, x.IsClosed });

            modelBuilder.Entity<Poll>()
                .HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Poll>()
                .HasOne(x => x.Creator)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PollOption>().HasIndex(x => x.PollId);

            modelBuilder.Entity<PollOption>()
                .HasOne(x => x.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollVote>().HasIndex(x => new { x.PollId, x.UserId });
            modelBuilder.Entity<PollVote>().HasIndex(x => new { x.PollOptionId, x.UserId }).IsUnique();

            modelBuilder.Entity<PollVote>()
                .HasOne(x => x.Poll)
                .WithMany()
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PollVote>()
                .HasOne(x => x.PollOption)
                .WithMany(o => o.Votes)
                .HasForeignKey(x => x.PollOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollVote>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Story>().HasIndex(x => x.UserId);
            modelBuilder.Entity<Story>().HasIndex(x => x.ExpiresAt);

            modelBuilder.Entity<Story>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryView>().HasIndex(x => new { x.StoryId, x.ViewerId }).IsUnique();

            modelBuilder.Entity<StoryView>()
                .HasOne(x => x.Story)
                .WithMany(s => s.Views)
                .HasForeignKey(x => x.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryView>()
                .HasOne(x => x.Viewer)
                .WithMany()
                .HasForeignKey(x => x.ViewerId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ClubRecommendationDismissal>()
                .HasIndex(x => new { x.UserId, x.ClubId })
                .IsUnique();

            modelBuilder.Entity<ClubRecommendationDismissal>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubRecommendationDismissal>()
                .HasOne<Club>()
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<LiveParticipant>().HasIndex(x => new { x.EventId, x.LeftAt });
            modelBuilder.Entity<LiveParticipant>().HasIndex(x => new { x.EventId, x.UserId });

            modelBuilder.Entity<LiveParticipant>()
                .HasOne(x => x.Event)
                .WithMany(e => e.LiveParticipants)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveParticipant>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LiveChatMessage>().HasIndex(x => new { x.EventId, x.SentAt });

            modelBuilder.Entity<LiveChatMessage>()
                .HasOne(x => x.Event)
                .WithMany(e => e.LiveChatMessages)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveChatMessage>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LiveModeration>()
                .HasIndex(x => new { x.EventId, x.UserId })
                .IsUnique();

            modelBuilder.Entity<LiveModeration>()
                .HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveModeration>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LiveModeration>()
                .HasOne(x => x.Moderator)
                .WithMany()
                .HasForeignKey(x => x.ModeratedBy)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Badge>().HasIndex(x => x.Code).IsUnique();

            modelBuilder.Entity<UserBadge>().HasIndex(x => new { x.UserId, x.BadgeId, x.ClubId }).IsUnique();

            modelBuilder.Entity<UserBadge>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(x => x.Badge)
                .WithMany(b => b.AwardedTo)
                .HasForeignKey(x => x.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SearchHistory>()
                .Property(x => x.Query)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<SearchHistory>()
                .HasIndex(x => new { x.UserId, x.Query })
                .IsUnique();

            modelBuilder.Entity<SearchHistory>().HasIndex(x => new { x.UserId, x.SearchedAt });

            modelBuilder.Entity<SearchHistory>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ClubInvite>().HasIndex(x => new { x.ClubId, x.InvitedUserId, x.Status });

            modelBuilder.Entity<ClubInvite>()
                .HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubInvite>()
                .HasOne(x => x.InvitedUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubInvite>()
                .HasOne(x => x.Inviter)
                .WithMany()
                .HasForeignKey(x => x.InvitedBy)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ClubInvite>()
                .HasIndex(x => new { x.ClubId, x.InvitedUserId })
                .HasFilter($"[{nameof(ClubInvite.Status)}] = 0")
                .IsUnique();
        }

        //internal async Task<ClubMember?> GetMembershipAsync(int targetUserId, int clubId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}