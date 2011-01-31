using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;

namespace FT.Model
{
	public interface ICommentRepository
    {
		void RecordVote(int commentid, byte vote, int userid);
		void SaveComment(int elementid, int? parentid, string comment, CommentType commenttype, int userid);
        Comment Comment(int elementid, CommentType commenttype);
	}


	public class CommentRepository : Repository, ICommentRepository
    {
        public Comment Comment(int cid, CommentType commenttype)
        {
            return DB.Comments.Single(q => q.CommentId == cid && q.CommentType == commenttype);
        }

		public void SaveComment(int elementid, int? parentid, string comment, CommentType commenttype, int userid)
		{
			DB.Comments.InsertOnSubmit(
				new Comment()
					{
						UserId = userid,
						ParentId = parentid,
						ItemId = elementid,
						Date = DateTime.Now,
						CommentType = commenttype,
						CommentText = comment
					}
				);
			DB.SubmitChanges();
		}

		public void RecordVote(int commentid, byte vote, int userid)
		{
			// check to see whether we already have this vote
			var thevote = DB.CommentVotes.SingleOrDefault(v => 
				v.CommentId == commentid &&
				v.UserId == userid
				);

			if (thevote != null)
			{
				// update the sucker
				thevote.Vote = vote;
			}
			else
			{
				// create new one
				thevote = new CommentVote()
					{
						CommentId = commentid,
						Date = DateTime.Now,
						UserId = userid,
						Vote = vote
					};
				DB.CommentVotes.InsertOnSubmit(thevote);
			}
			DB.SubmitChanges();
		}
    }
}
