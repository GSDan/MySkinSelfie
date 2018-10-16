using SkinSelfie.Repository;
using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SkinSelfie.WebAPI.Controllers.Site
{
    [Authorize]
    public class StudiesController : SiteBaseController
    {
        private SkinSelfieDB db = new SkinSelfieDB();

        // GET: Studies
        public async Task<ActionResult> Index()
        {
            if (!await IsAdmin()) return new HttpUnauthorizedResult();
            await LoadViewBag();

            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();

            List<ServiceData.Models.Study> found =
                _studyRepository.Search(st => st.Manager.Email == User.Identity.Name)
                .OrderByDescending(st => st.CreatedAt).ToList();

            List<Models.Study> toRet = new List<Models.Study>();
            foreach (ServiceData.Models.Study sh in found)
            {
                toRet.Add(Models.Study.ToAppModel(sh, true));
            }

            ViewData["Title"] = "Your Managed Studies";

            return View(toRet);
        }

        // GET: Studies/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!await IsAdmin()) return new HttpUnauthorizedResult();
            await LoadViewBag();

            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();
            ServiceData.Models.Study study = _studyRepository.GetById(id.Value);
            if (study == null)
            {
                return HttpNotFound();
            }

            return View(Models.Study.ToAppModel(study, false));
        }

        public struct AddParticipantStruct
        {
            public string Email { get; set; }
            public int Id { get; set; }
        }

        // POST: Studies/Details
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind(Include = "Email,Id")] AddParticipantStruct newPart)
        {
            IReadWriteRepository<ServiceData.Models.User> _userRepository = new UserRepository();
            ServiceData.Models.User res =
                await _userRepository.Search(u => u.Email == newPart.Email).FirstOrDefaultAsync();
            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();
            ServiceData.Models.Study study = _studyRepository.GetById(newPart.Id);

            if (study == null)
            {
                return RedirectToAction("Index");
            }
            else if (res != null
                && study.Manager.Email == User.Identity.Name
                && !study.StudyEnrolments.Any(en => en.UserId == res.Id))
            {
                Models.StudyEnrolment enrol = new Models.StudyEnrolment
                {
                    CreatedAt = DateTime.UtcNow,
                    Study = Models.Study.ToAppModel(study, false),
                    StudyId = study.Id,
                    Enrolled = true,
                    User = Models.User.ToAppModel(res),
                    UserId = res.Id
                };

                IReadWriteRepository<ServiceData.Models.StudyEnrolment> _enrolRepository = new StudyEnrolmentRepository();
                var serviceMod = Models.StudyEnrolment.ToServiceModel(enrol, true, true);
                var finalRes = _enrolRepository.Insert(serviceMod);
                return RedirectToAction("Details", new { id = study.Id });
            }

            return RedirectToAction("Details", new { id = study?.Id });
        }


        // GET: Studies/Create
        public async Task<ActionResult> Create()
        {
            if (!await IsAdmin()) return new HttpUnauthorizedResult();
            await LoadViewBag();

            return View();
        }

        // POST: Studies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Code")] Models.Study study)
        {
            IReadWriteRepository<ServiceData.Models.User> _userRepository = new UserRepository();
            ServiceData.Models.User thisUser =
                await _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();

            study.Active = true;
            study.CreatedAt = DateTime.UtcNow;
            study.Manager = Models.User.ToAppModel(thisUser);
            study.ManagerId = thisUser.Id;

            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();
            bool existing = await _studyRepository.Search(st => st.Code == study.Code).AnyAsync();

            if (ModelState.IsValid && !existing)
            {
                _studyRepository.Insert(Models.Study.ToServiceModel(study, true));
            }

            return RedirectToAction("Index");
        }

        // GET: Studies/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            await LoadViewBag();

            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();
            ServiceData.Models.Study study = _studyRepository.GetById(id.Value);
            if (study == null)
            {
                return HttpNotFound();
            }

            return View(Models.Study.ToAppModel(study, false));
        }

        // POST: Studies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            IReadWriteRepository<ServiceData.Models.Study> _studyRepository = new StudyRepository();
            await _studyRepository.Delete(id);
            return RedirectToAction("Index");
        }

        public struct UnenrollModel
        {
            public int enrolId { get; set; }
            public int studyId { get; set; }
        }

        // POST: Studies/Delete/5
        [HttpPost, ActionName("Unenroll")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnenrollFrom([Bind(Include = "enrolId,studyId")] UnenrollModel unenroll)
        {
            IReadWriteRepository<ServiceData.Models.StudyEnrolment> _enrolRepository = new StudyEnrolmentRepository();
            await _enrolRepository.Delete(unenroll.enrolId);
            return RedirectToAction("Details", new { id = unenroll.studyId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
