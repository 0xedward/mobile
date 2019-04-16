﻿using Bit.Core.Models.Data;
using Bit.Core.Models.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bit.Core.Models.Domain
{
    public class Cipher : Domain
    {
        public Cipher() { }

        public Cipher(CipherData obj, bool alreadyEncrypted = false, Dictionary<string, object> localData = null)
        {
            BuildDomainModel(this, obj, new HashSet<string>
            {
                "Id",
                "UserId",
                "OrganizationId",
                "FolderId",
                "Name",
                "Notes"
            }, alreadyEncrypted, new HashSet<string> { "Id", "UserId", "OrganizationId", "FolderId" });

            Type = obj.Type;
            Favorite = obj.Favorite;
            OrganizationUseTotp = obj.OrganizationUseTotp;
            Edit = obj.Edit;
            RevisionDate = obj.RevisionDate;
            CollectionIds = new HashSet<string>(obj.CollectionIds);
            LocalData = localData;

            switch(Type)
            {
                case Enums.CipherType.Login:
                    Login = new Login(obj.Login, alreadyEncrypted);
                    break;
                case Enums.CipherType.SecureNote:
                    SecureNote = new SecureNote(obj.SecureNote, alreadyEncrypted);
                    break;
                case Enums.CipherType.Card:
                    Card = new Card(obj.Card, alreadyEncrypted);
                    break;
                case Enums.CipherType.Identity:
                    Identity = new Identity(obj.Identity, alreadyEncrypted);
                    break;
                default:
                    break;
            }

            Attachments = obj.Attachments?.Select(a => new Attachment(a, alreadyEncrypted)).ToList();
            Fields = obj.Fields?.Select(f => new Field(f, alreadyEncrypted)).ToList();
            PasswordHistory = obj.PasswordHistory?.Select(ph => new PasswordHistory(ph, alreadyEncrypted)).ToList();
        }

        public string Id { get; set; }
        public string OrganizationId { get; set; }
        public string FolderId { get; set; }
        public CipherString Name { get; set; }
        public CipherString Notes { get; set; }
        public Enums.CipherType Type { get; set; }
        public bool Favorite { get; set; }
        public bool OrganizationUseTotp { get; set; }
        public bool Edit { get; set; }
        public DateTime RevisionDate { get; set; }
        public Dictionary<string, object> LocalData { get; set; }
        public Login Login { get; set; }
        public Identity Identity { get; set; }
        public Card Card { get; set; }
        public SecureNote SecureNote { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<Field> Fields { get; set; }
        public List<PasswordHistory> PasswordHistory { get; set; }
        public HashSet<string> CollectionIds { get; set; }

        public async Task<CipherView> DecryptAsync()
        {
            var model = new CipherView(this);
            await DecryptObjAsync(model, this, new HashSet<string>
            {
                "Name",
                "Notes"
            }, OrganizationId);

            switch(Type)
            {
                case Enums.CipherType.Login:
                    model.Login = await Login.DecryptAsync(OrganizationId);
                    break;
                case Enums.CipherType.SecureNote:
                    model.SecureNote = await SecureNote.DecryptAsync(OrganizationId);
                    break;
                case Enums.CipherType.Card:
                    model.Card = await Card.DecryptAsync(OrganizationId);
                    break;
                case Enums.CipherType.Identity:
                    model.Identity = await Identity.DecryptAsync(OrganizationId);
                    break;
                default:
                    break;
            }

            if(Attachments?.Any() ?? false)
            {
                model.Attachments = new List<AttachmentView>();
                var tasks = new List<Task>();
                foreach(var attachment in Attachments)
                {
                    var t = attachment.DecryptAsync(OrganizationId)
                        .ContinueWith(async decAttachment => model.Attachments.Add(await decAttachment));
                    tasks.Add(t);
                }
                await Task.WhenAll(tasks);
            }
            if(Fields?.Any() ?? false)
            {
                model.Fields = new List<FieldView>();
                var tasks = new List<Task>();
                foreach(var field in Fields)
                {
                    var t = field.DecryptAsync(OrganizationId)
                        .ContinueWith(async decField => model.Fields.Add(await decField));
                    tasks.Add(t);
                }
                await Task.WhenAll(tasks);
            }
            if(PasswordHistory?.Any() ?? false)
            {
                model.PasswordHistory = new List<PasswordHistoryView>();
                var tasks = new List<Task>();
                foreach(var ph in PasswordHistory)
                {
                    var t = ph.DecryptAsync(OrganizationId)
                        .ContinueWith(async decPh => model.PasswordHistory.Add(await decPh));
                    tasks.Add(t);
                }
                await Task.WhenAll(tasks);
            }
            return model;
        }

        public CipherData ToCipherData(string userId)
        {
            var c = new CipherData
            {
                Id = Id,
                OrganizationId = OrganizationId,
                FolderId = FolderId,
                UserId = OrganizationId != null ? userId : null,
                Edit = Edit,
                OrganizationUseTotp = OrganizationUseTotp,
                Favorite = Favorite,
                RevisionDate = RevisionDate,
                Type = Type,
                CollectionIds = CollectionIds.ToList()
            };
            BuildDataModel(this, c, new HashSet<string>
            {
                "Name",
                "Notes"
            });
            switch(c.Type)
            {
                case Enums.CipherType.Login:
                    c.Login = Login.ToLoginData();
                    break;
                case Enums.CipherType.SecureNote:
                    c.SecureNote = SecureNote.ToSecureNoteData();
                    break;
                case Enums.CipherType.Card:
                    c.Card = Card.ToCardData();
                    break;
                case Enums.CipherType.Identity:
                    c.Identity = Identity.ToIdentityData();
                    break;
                default:
                    break;
            }
            c.Fields = Fields?.Select(f => f.ToFieldData()).ToList();
            c.Attachments = Attachments?.Select(a => a.ToAttachmentData()).ToList();
            c.PasswordHistory = PasswordHistory?.Select(ph => ph.ToPasswordHistoryData()).ToList();
            return c;
        }
    }
}
