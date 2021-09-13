
  select top 200 *
  from [StackOverflow2010].[dbo].[Posts] p inner join [StackOverflow2010].[dbo].Users u
	on p.OwnerUserId = u.Id
	order by p.CreationDate desc