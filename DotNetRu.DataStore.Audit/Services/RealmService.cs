﻿using DotNetRu.DataStore.Audit.XmlEntities;

namespace DotNetRu.DataStore.Audit.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using AutoMapper;

    using DotNetRu.DataStore.Audit.Extensions;
    using DotNetRu.DataStore.Audit.Models;
    using DotNetRu.DataStore.Audit.RealmModels;

    using Realms;

    public class RealmService
    {
        private const string RealmResourceName = "DotNetRu.DataStore.Audit.Audit.realm";
        private static Realm auditRealm;

        public static Realm AuditRealm => auditRealm ?? (auditRealm = Realm.GetInstance("Audit.realm"));

        public static void Initialize()
        {
            InitializeRealm();
            InitializeAutoMapper();
        }

        public static IEnumerable<TAppModel> Get<TAppModel>()
        {
            var realmType = Mapper.Configuration.GetAllTypeMaps().Single(x => x.DestinationType == typeof(TAppModel)).SourceType;

            return AuditRealm.All(realmType.Name).AsEnumerable().Select(Mapper.Map<TAppModel>);
        }

        public static void Put(RealmObject contents)
        {
            AuditRealm.Write(() =>
            {
                AuditRealm.Add(contents, update: true);
            });
        }

        public static byte[] ExtractResource(string resourceName)
        {
            var assembly = typeof(RealmService).Assembly;
            using (Stream resFilestream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resFilestream == null)
                {
                    return null;
                }

                byte[] resultBytes = new byte[resFilestream.Length];
                resFilestream.Read(resultBytes, 0, resultBytes.Length);
                return resultBytes;
            }
        }

        private static void InitializeRealm()
        {
            var realmDB = "Audit.realm";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            File.WriteAllBytes(Path.Combine(documentsPath, realmDB), ExtractResource(RealmResourceName));
        }

        private static void InitializeAutoMapper()
        {
            Mapper.Reset();
            Mapper.Initialize(
                cfg =>
                    {
                        cfg.CreateMap<Speaker, SpeakerModel>().ConvertUsing(x => x.ToModel());
                        cfg.CreateMap<Venue, VenueModel>().ConvertUsing(x => x.ToModel());
                        cfg.CreateMap<Friend, FriendModel>().ConvertUsing(x => x.ToModel());
                        cfg.CreateMap<Talk, TalkModel>().ConvertUsing(x => x.ToModel());
                        cfg.CreateMap<Meetup, MeetupModel>().ConvertUsing(x => x.ToModel());

                        cfg.CreateMap<SpeakerEntity, Speaker>().AfterMap(
                        (src, dest) =>
                        {
                            // dest.Avatar = AuditHelper.LoadImage("speakers", src.Id, "avatar.jpg");
                        });
                        cfg.CreateMap<VenueEntity, Venue>();
                        cfg.CreateMap<FriendEntity, Friend>().AfterMap(
                            (src, dest) =>
                            {
                                var friendId = src.Id;

                            //dest.LogoSmall = AuditHelper.LoadImage("friends", friendId, "logo.small.png");
                            //dest.Logo = AuditHelper.LoadImage("friends", friendId, "logo.png");
                        });
                        cfg.CreateMap<CommunityEntity, Community>();
                        cfg.CreateMap<TalkEntity, Talk>().AfterMap(
                            (src, dest) =>
                            {
                                foreach (string speakerId in src.SpeakerIds)
                                {
                                    var speaker = RealmService.AuditRealm.Find<Speaker>(speakerId);

                                    dest.Speakers.Add(speaker);
                                }
                            });
                        cfg.CreateMap<MeetupEntity, Meetup>().AfterMap(
                            (src, dest) =>
                            {
                                foreach (string talkId in src.TalkIds)
                                {
                                    var talk = RealmService.AuditRealm.Find<Talk>(talkId);
                                    dest.Talks.Add(talk);
                                }

                                foreach (string friendId in src.FriendIds)
                                {
                                    var friend = RealmService.AuditRealm.Find<Friend>(friendId);
                                    dest.Friends.Add(friend);
                                }

                                dest.Venue = RealmService.AuditRealm.Find<Venue>(src.VenueId);
                            });
                    });
        }
    }
}
