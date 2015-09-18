using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using Yarn.Extensions;
using Yarn.Linq.Expressions;
using Yarn.Specification;

namespace Yarn.Adapters
{
    public class MultiTenantRepository : RepositoryAdapter
    {
        private readonly ITenant _owner;

        public MultiTenantRepository(IRepository repository, ITenant owner)
            : base(repository)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            _owner = owner;
        }
        
        public override T GetById<T, ID>(ID id)
        {
            var result = base.GetById<T, ID>(id);
            var tenant = result as ITenant;
            if (tenant == null)
            {
                return result;
            }
            return tenant.TenantId != _owner.TenantId ? null : result;
        }
        
        public override T Find<T>(ISpecification<T> criteria) 
        {
            return Find(((Specification<T>)criteria).Predicate);
        }

        public override T Find<T>(Expression<Func<T, bool>> criteria)
        {
            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;

            return typeof(ITenant).IsAssignableFrom(typeof(T)) ? base.All<T>().Where(CastRemoverVisitor<ITenant>.Convert(filter)).FirstOrDefault(criteria) : base.Find(criteria);
        }

        public override IEnumerable<T> FindAll<T>(ISpecification<T> criteria, int offset = 0, int limit = 0, Sorting<T> orderBy = null)
        {
            if (!typeof(ITenant).IsAssignableFrom(typeof(T)))
            {
                return base.FindAll(criteria, offset, limit, orderBy);
            }

            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;
            var spec = ((Specification<T>)criteria).And(CastRemoverVisitor<ITenant>.Convert(filter));
            var query = base.All<T>().Where(spec.Predicate);
            return this.Page(query, offset, limit, orderBy);
        }

        public override IEnumerable<T> FindAll<T>(Expression<Func<T, bool>> criteria, int offset = 0, int limit = 0, Sorting<T> orderBy = null)
        {
            if (!typeof(ITenant).IsAssignableFrom(typeof(T)))
            {
                return base.FindAll(criteria, offset, limit, orderBy);
            }

            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;
            var spec = new Specification<T>(CastRemoverVisitor<ITenant>.Convert(filter)).And(criteria);
            var query = base.All<T>().Where(spec.Predicate);
            return this.Page(query, offset, limit, orderBy);
        }

        public override IList<T> Execute<T>(string command, ParamList parameters)
        {
            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;
            return typeof(ITenant).IsAssignableFrom(typeof(T)) ? base.Execute<T>(command, parameters).Where(CastRemoverVisitor<ITenant>.Convert(filter).Compile()).ToArray() : base.Execute<T>(command, parameters);
        }

        public override T Add<T>(T entity)
        {
            var tenant = entity as ITenant;
            if (tenant == null)
            {
                return base.Add(entity);
            }

            if (tenant.TenantId == _owner.TenantId)
            {
                return base.Add(entity);
            }
            throw new InvalidOperationException();
        }

        public override T Remove<T>(T entity)
        {
            var tenant = entity as ITenant;
            if (tenant == null)
            {
                return base.Remove(entity);
            }

            if (tenant.TenantId == _owner.TenantId)
            {
                return base.Remove(entity);
            }
            throw new InvalidOperationException();
        }

        public override T Remove<T, ID>(ID id)
        {
            if (typeof(ITenant).IsAssignableFrom(typeof(T)))
            {
                var entity = base.GetById<T, ID>(id);
                if (((ITenant)entity).TenantId == _owner.TenantId)
                {
                    return base.Remove(entity);
                }
                throw new InvalidOperationException();
            }
            return base.Remove<T, ID>(id);
        }

        public override T Update<T>(T entity)
        {
            var tenant = entity as ITenant;
            if (tenant == null)
            {
                return base.Update(entity);
            }

            if (tenant.TenantId == _owner.TenantId)
            {
                return base.Update(entity);
            }
            throw new InvalidOperationException();
        }

        public override long Count<T>()
        {
            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;
            return typeof(ITenant).IsAssignableFrom(typeof(T)) ? base.All<T>().LongCount(CastRemoverVisitor<ITenant>.Convert(filter)) : base.Count<T>();
        }

        public override long Count<T>(ISpecification<T> criteria)
        {
            return FindAll(criteria).AsQueryable().LongCount();
        }

        public override long Count<T>(Expression<Func<T, bool>> criteria)
        {
            return FindAll(criteria).AsQueryable().LongCount();
        }

        public override IQueryable<T> All<T>()
        {
            Expression<Func<T, bool>> filter = e => ((ITenant)e).TenantId == _owner.TenantId;
            return typeof(ITenant).IsAssignableFrom(typeof(T)) ? base.All<T>().Where(CastRemoverVisitor<ITenant>.Convert(filter)) : base.All<T>();
        }

        public override void Detach<T>(T entity)
        {
            var tenant = entity as ITenant;
            if (tenant != null)
            {
                if (tenant.TenantId == _owner.TenantId)
                {
                    base.Detach(entity);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                base.Detach(entity);
            }
        }

        public override void Attach<T>(T entity)
        {
            var tenant = entity as ITenant;
            if (tenant != null)
            {
                if (tenant.TenantId == _owner.TenantId)
                {
                    base.Attach(entity);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                base.Attach(entity);
            }
        }
        
        public long TenantId
        {
            get { return _owner.TenantId; }
        }

        public long OwnerId
        {
            get { return _owner.OwnerId; }
        }

        public override ILoadService<T> Load<T>()
        {
            if (typeof (ITenant).IsAssignableFrom(typeof (T)))
                return new LoadService<T>(base.Load<T>());
            return base.Load<T>();
        }

        private class LoadService<T> : ILoadService<T>
            where T : class, ITenant
        {
            private readonly ILoadService<T> _loadService;
            private readonly ITenant _owner;

            public LoadService(ILoadService<T> loadService, ITenant owner)
            {
                _loadService = loadService;
                _owner = owner;
            }

            public IQueryable<T> All()
            {
                return _loadService.All().Where(t => t.TenantId == _owner.TenantId);
            }

            public void Dispose()
            {
                _loadService.Dispose();
            }

            public ILoadService<T> Include<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
            {
                ILoadService<T> loadService = _loadService.Include(path);
                if (loadService == _loadService)
                    return this;
                return new LoadService<T>(loadService, _owner);
            }

            public T Update(T entity)
            {
                var tenant = entity as ITenant;
                if (tenant == null || tenant.TenantId == _owner.TenantId)
                {
                    return _loadService.Update(entity);
                }

                throw new InvalidOperationException();
            }

            public T Find(Expression<Func<T, bool>> criteria)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, int offset = 0, int limit = 0, Sorting<T> orderBy = null)
            {
                throw new NotImplementedException();
            }

            public T Find(ISpecification<T> criteria)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<T> FindAll(ISpecification<T> criteria, int offset = 0, int limit = 0, Sorting<T> orderBy = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
