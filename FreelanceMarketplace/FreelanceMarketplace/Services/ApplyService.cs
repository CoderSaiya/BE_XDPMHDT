﻿using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Services.Implementations
{
    public class ApplyService : IApplyService
    {
        private readonly AppDbContext _context;

        public ApplyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Apply> CreateApplyAsync(Apply apply)
        {
            try
            {
                await _context.Applies.AddAsync(apply);
                await _context.SaveChangesAsync();
                return apply;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating apply", ex);
            }
        }

        public async Task<Apply> GetApplyByIdAsync(int applyId)
        {
            try
            {
                return await _context.Applies
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .SingleOrDefaultAsync(a => a.ApplyId == applyId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving apply with ID {applyId}", ex);
            }
        }

        public async Task<IEnumerable<Apply>> GetAppliesForProjectAsync(int projectId)
        {
            try
            {
                return await _context.Applies
                    .Include(a => a.User)
                    .Where(a => a.ProjectId == projectId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving applies for project with ID {projectId}", ex);
            }
        }

        public async Task<Apply> UpdateApplyAsync(Apply apply)
        {
            try
            {
                var existingApply = await _context.Applies.FindAsync(apply.ApplyId);
                if (existingApply == null)
                    throw new KeyNotFoundException("Apply not found");

                _context.Entry(existingApply).CurrentValues.SetValues(apply);
                await _context.SaveChangesAsync();
                return existingApply;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating apply", ex);
            }
        }

        public async Task<bool> DeleteApplyAsync(int applyId)
        {
            try
            {
                var apply = await _context.Applies.FindAsync(applyId);
                if (apply == null) return false;

                _context.Applies.Remove(apply);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting apply", ex);
            }
        }
    }
}
