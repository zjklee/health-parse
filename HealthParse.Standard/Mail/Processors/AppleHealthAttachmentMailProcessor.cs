﻿using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health;
using MimeKit;

namespace HealthParse.Standard.Mail.Processors
{
    public class AppleHealthAttachmentMailProcessor : IMailProcessor
    {
        private readonly string _from;
        private readonly Settings.Settings _settings;

        public AppleHealthAttachmentMailProcessor(string from, Settings.Settings settings)
        {
            _from = @from;
            _settings = settings;
        }
        public Result<MimeMessage> Process(MimeMessage originalEmail, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            var exportAttachment = attachments.Single(a => a.Item1 == "export.zip");

            var attachment = ExcelReport.CreateReport(exportAttachment.Item2, _settings);
            var attachmentName = $"export.{originalEmail.Date.Date:yyyy-mm-dd}.xlsx";

            var reply = MailUtility.ConstructReply(originalEmail, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = @"Hey there, I saw your health data... good work!";
                builder.Attachments.Add(attachmentName, attachment);
            });

            return Result.Success(reply);
        }
        public bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return attachments.Any(a => a.Item1 == "export.zip");
        }
    }
}