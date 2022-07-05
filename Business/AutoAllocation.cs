using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AllocationSystem.WebApi.Business
{
    public class AutoAllocation : IAutoAllocation
    {
        #region "Private Variables"
        private readonly AllocationSystemDbContext _context;

        private static long _UserID;
        private static AdminSetting Config { get; set; }
        private static int GroupCount { get; set; }
        private static Dictionary<long, List<long>> StudentPref { get; set; }
        private static Dictionary<long, bool> UnallocatedStudent { get; set; }
        private static Dictionary<long, bool> UnallocatedTopic { get; set; }
        private static List<AllocatedTopic> AllocatedTopics { get; set; }

        #endregion

        public AutoAllocation(AllocationSystemDbContext context)
        {
            _context = context;
        }

        public async Task<AllocationHistory> RunAllocationProcess(long UserID)
        {
            var log = new AllocationHistory { ProcessStartDateTime = DateTime.Now };
            try
            {
                InitializingData(UserID);
                MatchingPreferenceAllocation();
                AllocationForNonPreferenceStudents();
                AssignGroupNameAndSupervisor();
                log.IsSuccess = true;
            }
            catch (Exception ex)
            {
                log.Error = ex.Message;
                log.IsSuccess = false;
            }
            finally
            {
                log.ProcessEndDateTime = DateTime.Now;
                if (log.IsSuccess)
                {
                    StoreResults();
                }
                log.CreatedBy = UserID;
                log.CreatedDate = DateTime.Now;
                _context.AllocationHistories.Add(log);
                _context.SaveChanges();
            }
            return await Task.FromResult(log);
        }

        #region  "Initializing variables used for allocating topic"
        private void InitializingData(long UserID)
        {
            //Data Cleaning
            foreach (var entity in _context.Groups)
            {
                _context.Groups.Remove(entity);
            }
            foreach (var entity in _context.Students)
            {
                entity.GroupID = null;
                entity.TopicID = null;
            }
            _context.SaveChanges();

            //Admin Configuration
            Config = _context.AdminSettings.FirstOrDefault();

            //Student list
            UnallocatedStudent = new Dictionary<long, bool>();
            UnallocatedStudent = _context.Students.Where(x => x.IsActive).ToDictionary(c => c.ID, c => false);

            //Student's Preference list
            StudentPref = _context.Preferences.AsEnumerable()//.Where(s => s.Student.TopicID == null && s.Student.GroupID == null)
                             .OrderBy(c => c.CreatedDate)
                             .GroupBy(s => s.StudentID)
                             .ToDictionary(a => a.Key, a => a.OrderBy(a => a.PreferenceOrder).Select(h => h.TopicID).ToList());

            //select PreferenceOrder,StudentID from Preference group by PreferenceOrder,CreatedDate,StudentID

            //Topic list
            UnallocatedTopic = new Dictionary<long, bool>();
            UnallocatedTopic = _context.Topics.ToDictionary(c => c.TopicID, c => false);

            //GroupCount = _context.Groups.Count() + 1;
            GroupCount = 1;
            _UserID = UserID;
            AllocatedTopics = new List<AllocatedTopic>();
            //AllocatedTopics_SPO = new List<AllocatedTopic>();

            //UnallocatedStudent_SPO = UnallocatedStudent;
            //UnallocatedTopic_SPO = UnallocatedTopic;
        }
        #endregion

        #region "Evaluation - Method to Allocate Topics to Students"
        private void MatchingPreferenceAllocation()
        {
            bool IsSuccess = false;

            foreach (var (StudentID, PreferenceList) in StudentPref)
            {
                if (IsStudentFree(StudentID))
                {
                    foreach (var pref in PreferenceList)
                    {
                        if (IsTopicAvailable(pref))
                        {
                            IsSuccess = AllocateTopicToStudent(pref, StudentID);
                            if (IsSuccess)
                            {
                                UnallocatedStudent[StudentID] = IsSuccess;
                                IsSuccess = false;
                                break;
                            }
                        }
                    }
                }
            }                    
        }

        private static void AllocationForNonPreferenceStudents()
        {
            bool IsSuccess = false;

            //allocated topics - the students who are not chosen preferences (vs) topic which have less group size 
            var topicsWithLessTeamSize = AllocatedTopics.Where(s => s.StudentID.Count < Config.TeamSize)
                                                                    .OrderByDescending(d => d.StudentID.Count);
            if (GetUnallocatedStudentCount() > 0 && topicsWithLessTeamSize.Any())
            {
                foreach (var (StudentID, IsStudentFree) in UnallocatedStudent)
                {
                    if (!IsStudentFree)
                    {
                        foreach (var alloc in topicsWithLessTeamSize)
                        {
                            if (IsTopicAvailable(alloc.TopicID))
                            {
                                IsSuccess = AllocateTopicToStudent(alloc.TopicID, StudentID);
                                if (IsSuccess)
                                {
                                    UnallocatedStudent[StudentID] = IsSuccess;
                                    IsSuccess = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //the students who are not chosen preferences vs unallocated topics
            if (GetUnallocatedStudentCount() > 0)
            {
                foreach (var (TopicID, IsAvailable) in UnallocatedTopic)
                {
                    if (!IsAvailable || Config.IsTopicMultiple)
                    {
                        int i = 1;
                        foreach (var (StudentID, IsStudentFree) in UnallocatedStudent)
                        {
                            if (!IsStudentFree)
                            {
                                if (IsTopicAvailable(TopicID))
                                {
                                    IsSuccess = AllocateTopicToStudent(TopicID, StudentID);
                                    if (IsSuccess)
                                    {
                                        UnallocatedStudent[StudentID] = IsSuccess;
                                        IsSuccess = false;
                                        if (i == Config.TeamSize)
                                        {
                                            break;
                                        }
                                        i += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        #endregion

        #region "Evaluation - Sub Methods"

        #region Get Unallocated Student Count
        private static int GetUnallocatedStudentCount()
        {
            return UnallocatedStudent.Values.Where(r => r == false).Count();
        }
        #endregion

        #region Check whether topic is available or not if available then proceed allocaion process
        private static bool IsTopicAvailable(long TopicId)
        {
            return IsSeatAvailablePerGroup(TopicId, Config.IsTopicMultiple ? Config.NoOfGroups : 1);
        }
        private static bool IsSeatAvailablePerGroup(long TopicId, int Noofgrp)
        {
            var NoOfGroupExists = AllocatedTopics.Where(x => x.TopicID == TopicId);
            if (NoOfGroupExists.Any())
            {
                if (AllocatedTopics.Where(x => x.TopicID == TopicId && x.GroupName == null && x.StudentID.Count < Config.TeamSize).Any())
                    return true;
                else
                {
                    return NoOfGroupExists.Count() < Noofgrp;
                }
            }
            else return !UnallocatedTopic[TopicId];
        }
        #endregion

        #region Verify student is available or not
        private static bool IsStudentFree(long stuId)
        {
            return UnallocatedStudent.ContainsKey(stuId) && !UnallocatedStudent[stuId];
        }
        #endregion

        #region Assigning topic to student
        private static bool AllocateTopicToStudent(long TopicId, long stuId)
        {
            if (IsTopicExists(TopicId))
            {
                return AddStudentsToExistingTopic(TopicId, stuId);
            }
            else
            {
                return AddStudentAndTopic(TopicId, stuId);
            }
        }
        #endregion

        #region Check whether topic exist in Allocation Topics list
        private static bool IsTopicExists(long TopicId)
        {
            return AllocatedTopics.Where(k => k.TopicID == TopicId && string.IsNullOrEmpty(k.GroupName)).Any();
        }
        #endregion

        #region Add if topic not exist in Allocated Topic list
        private static bool AddStudentAndTopic(long TopicId, long stuId)
        {
            AllocatedTopics.Add(new AllocatedTopic { TopicID = TopicId, StudentID = new List<long> { stuId } });
            UnallocatedTopic[TopicId] = true;
            UpdateGroupNameIfFull(TopicId);
            return true;
        }
        #endregion

        #region Update if topic exist in Allocated Topic list
        private static bool AddStudentsToExistingTopic(long TopicId, long stuId)
        {
            AllocatedTopics.First(d => d.TopicID == TopicId && d.GroupName == null).StudentID.Add(stuId);
            UpdateGroupNameIfFull(TopicId);
            return true;
        }
        #endregion

        #region Add Group Name if the last person added to the group
        private static void UpdateGroupNameIfFull(long TopicId)
        {
            var isfull = AllocatedTopics.Where(c => c.TopicID == TopicId && c.GroupName == null && c.StudentID.Count == Config.TeamSize).Any();
            if (isfull)
            {
                var NewGrpName = GetGeneratedGroupName(GroupCount);
                AllocatedTopics.First(d => d.TopicID == TopicId && d.GroupName == null).GroupName = NewGrpName;
                GroupCount += 1;
            }
        }
        #endregion

        #region Group Name Generator
        public static String GetGeneratedGroupName(int count)
        {
            count--;
            String col = Convert.ToString((char)('A' + (count % 26)));
            while (count >= 26)
            {
                count = (count / 26) - 1;
                col = Convert.ToString((char)('A' + (count % 26))) + col;
            }
            return col;
        }
        #endregion

        #endregion

        #region "Assigning GroupNames And Supervisors to the allocation list"
        private void AssignGroupNameAndSupervisor()
        {
            AssignGroupNames();
            AllocateSupervisors();
        }
        private void AssignGroupNames()
        {
            //Update group name if not exists
            var UpdateGroupName = AllocatedTopics.Where(s => s.GroupName == null);
            foreach (var grp in UpdateGroupName)
            {
                var NewGrpName = GetGeneratedGroupName(GroupCount);
                grp.GroupName = NewGrpName;
                GroupCount += 1;
            }
        }
        private void AllocateSupervisors()
        {
            var supchoic = _context.SupervisorChoices.Where(x => x.Supervisors.IsActive).ToList();
            var sup = supchoic.GroupBy(c => c.SupervisorID)
                .ToDictionary(x => x.Key, a => a.Select(h => new SupChoice { TopicID = h.TopicID, IsAssigned = false }).ToList());

            foreach (var grp in AllocatedTopics)
            {
                foreach (var (SupID, Topic) in sup)
                {
                    if (grp.TopicID == Topic.Where(d => d.TopicID == grp.TopicID && d.IsAssigned == false).Select(v => v.TopicID).FirstOrDefault())
                    {
                        var account = AllocatedTopics.FirstOrDefault(a => a.TopicID == grp.TopicID);
                        if (account.SupervisorID == 0)
                        {
                            if (account != null)
                                account.SupervisorID = SupID;
                            var topi = Topic.FirstOrDefault(a => a.TopicID == grp.TopicID);
                            if (topi != null)
                                topi.IsAssigned = true;
                        }
                    }
                }
            }
        }
        #endregion

        #region "Store Results"
        private void StoreResults()
        {
            foreach (var res in AllocatedTopics)
            {
                var grp = _context.Groups.Where(o => o.GroupName == res.GroupName).SingleOrDefault();
                long id = 0;

                if (grp != null)
                {
                    id = grp.GroupID;
                    grp.TopicID = res.TopicID;
                    grp.SupervisorID = res.SupervisorID;
                    grp.LastUpdatedBy = _UserID;
                    grp.LastUpdatedDate = DateTimeOffset.UtcNow;
                    _context.Entry(grp).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    var newgrp = new Group
                    {
                        GroupName = res.GroupName,
                        TopicID = res.TopicID,
                        SupervisorID = res.SupervisorID,
                        CreatedBy = _UserID,
                        CreatedDate = DateTimeOffset.UtcNow,
                    };
                    _context.Groups.Add(newgrp);
                    _context.SaveChanges();
                    id = newgrp.GroupID;
                }
                foreach (var stud in res.StudentID)
                {
                    var stu = _context.Students.Where(o => o.ID == stud && o.IsActive).SingleOrDefault();
                    if (stu != null)
                    {
                        stu.TopicID = res.TopicID;
                        stu.GroupID = id;
                        stu.LastUpdatedBy = _UserID;
                        stu.LastUpdatedDate = DateTimeOffset.UtcNow;
                        _context.Entry(stu).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
        }
        #endregion
    }

}
