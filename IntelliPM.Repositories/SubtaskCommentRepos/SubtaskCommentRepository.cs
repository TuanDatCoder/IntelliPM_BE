﻿using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskCommentRepos
{
    public class SubtaskCommentRepository : ISubtaskCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public SubtaskCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }
        public async Task Add(SubtaskComment subtaskComment)
        {
            await _context.SubtaskComment.AddAsync(subtaskComment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(SubtaskComment subtaskComment)
        {
            _context.SubtaskComment.Remove(subtaskComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubtaskComment>> GetAllSubtaskComment()
        {
            return await _context.SubtaskComment
                .Include(tf => tf.Account)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<SubtaskComment?> GetByIdAsync(int id)
        {
            return await _context.SubtaskComment
                .Include(tf => tf.Account)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Update(SubtaskComment subtaskComment)
        {
            _context.SubtaskComment.Update(subtaskComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubtaskComment>> GetSubtaskCommentBySubtaskIdAsync(string subtaskId)
        {
            return await _context.SubtaskComment
                .Where(tf => tf.SubtaskId == subtaskId)
                .Include(tf => tf.Account)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}
