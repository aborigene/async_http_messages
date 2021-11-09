CREATE DATABASE `pix`;

USE `pix`;

DROP TABLE IF EXISTS `pix_operation`;
CREATE TABLE `pix_operation` (
  `id` int NOT NULL AUTO_INCREMENT,
  `Date` bigint NOT NULL,
  `Value` int NOT NULL,
  `End_To_EndID` varchar(1000) not null,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `dynatrace_correlation`;
CREATE TABLE `dynatrace_correlation` (
  `End_To_EndID` varchar(32) not null,
  `dt_tag` varchar(1000) not null, 
  `start_time` bigint not null,
  PRIMARY KEY (`End_To_EndID`)
);

