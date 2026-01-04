-- ========================================
-- ECOMMERCE MODULAR MONOLITH DATABASE
-- Versão consolidada para monolito modular
-- ========================================
-- 
-- ARQUITETURA:
-- Este schema consolida 6 módulos de domínio em um único banco de dados,
-- mantendo a separação lógica através de prefixos de tabela e schemas.
-- 
-- MÓDULOS:
-- 1. users     - Usuários, perfis, endereços, sessões, notificações
-- 2. catalog   - Categorias, produtos, imagens, estoque, avaliações
-- 3. cart      - Carrinhos e itens
-- 4. orders    - Pedidos, itens, histórico, rastreamento
-- 5. payments  - Pagamentos, métodos, transações
-- 6. coupons   - Cupons e regras de desconto
--
-- DIFERENÇAS DO MICROSERVIÇOS:
-- - FKs reais entre módulos (integridade referencial)
-- - Outbox/Inbox consolidado (opcional, para eventos internos)
-- - Audit log unificado
-- - Enums compartilhados onde aplicável
-- ========================================

-- ========================================
-- PARTE 1: INFRAESTRUTURA COMPARTILHADA
-- ========================================

-- ========================================
-- 1.1 EXTENSIONS
-- ========================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "citext";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ========================================
-- 1.2 SCHEMAS (separação lógica dos módulos)
-- ========================================
CREATE SCHEMA IF NOT EXISTS users;
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS cart;
CREATE SCHEMA IF NOT EXISTS orders;
CREATE SCHEMA IF NOT EXISTS payments;
CREATE SCHEMA IF NOT EXISTS coupons;
CREATE SCHEMA IF NOT EXISTS shared;

-- Define search_path para facilitar queries
-- Em produção, configure conforme necessidade
SET search_path TO public, users, catalog, cart, orders, payments, coupons, shared;

-- ========================================
-- 1.3 FUNÇÕES COMPARTILHADAS
-- ========================================

-- Função para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION shared.trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Função para incrementar version (optimistic locking)
CREATE OR REPLACE FUNCTION shared.trigger_increment_version()
RETURNS TRIGGER AS $$
BEGIN
    NEW.version = OLD.version + 1;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Função para gerar slug a partir do nome
CREATE OR REPLACE FUNCTION shared.generate_slug(input_text TEXT)
RETURNS TEXT AS $$
BEGIN
    RETURN LOWER(
        REGEXP_REPLACE(
            REGEXP_REPLACE(
                TRANSLATE(
                    input_text,
                    'áàâãäéèêëíìîïóòôõöúùûüçñÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇÑ',
                    'aaaaaeeeeiiiiooooouuuucnAAAAAEEEEIIIIOOOOOUUUUCN'
                ),
                '[^a-zA-Z0-9\s-]', '', 'g'
            ),
            '\s+', '-', 'g'
        )
    );
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- 1.4 ENUMS COMPARTILHADOS
-- ========================================

-- Tipos de movimento de estoque
CREATE TYPE shared.stock_movement_type AS ENUM ('IN', 'OUT', 'ADJUSTMENT', 'RESERVE', 'RELEASE');

-- Status de produto
CREATE TYPE shared.product_status AS ENUM ('DRAFT', 'ACTIVE', 'INACTIVE', 'OUT_OF_STOCK', 'DISCONTINUED');

-- Status de carrinho
CREATE TYPE shared.cart_status AS ENUM ('ACTIVE', 'MERGED', 'CONVERTED', 'ABANDONED', 'EXPIRED');

-- Status de pedido
CREATE TYPE shared.order_status AS ENUM (
    'PENDING', 'PAYMENT_PROCESSING', 'PAID', 'PREPARING', 
    'SHIPPED', 'OUT_FOR_DELIVERY', 'DELIVERED', 
    'CANCELLED', 'REFUNDED', 'FAILED'
);

-- Métodos de pagamento
CREATE TYPE shared.payment_method_type AS ENUM (
    'CREDIT_CARD', 'DEBIT_CARD', 'PIX', 'BOLETO', 'WALLET', 'BANK_TRANSFER'
);

-- Métodos de envio
CREATE TYPE shared.shipping_method AS ENUM ('STANDARD', 'EXPRESS', 'SAME_DAY', 'PICKUP');

-- Motivos de cancelamento
CREATE TYPE shared.cancellation_reason AS ENUM (
    'CUSTOMER_REQUEST', 'PAYMENT_FAILED', 'OUT_OF_STOCK', 
    'FRAUD_SUSPECTED', 'SHIPPING_ISSUE', 'OTHER'
);

-- Status de pagamento
CREATE TYPE shared.payment_status AS ENUM (
    'PENDING', 'PROCESSING', 'AUTHORIZED', 'CAPTURED', 
    'FAILED', 'CANCELLED', 'REFUNDED', 'PARTIALLY_REFUNDED', 
    'CHARGEBACK', 'EXPIRED'
);

-- Bandeiras de cartão
CREATE TYPE shared.card_brand AS ENUM (
    'VISA', 'MASTERCARD', 'AMEX', 'ELO', 
    'HIPERCARD', 'DINERS', 'DISCOVER', 'JCB', 'OTHER'
);

-- Tipos de transação de pagamento
CREATE TYPE shared.transaction_type AS ENUM (
    'AUTHORIZATION', 'CAPTURE', 'VOID', 'REFUND', 'CHARGEBACK'
);

-- Tipos de cupom
CREATE TYPE shared.coupon_type AS ENUM (
    'PERCENTAGE', 'FIXED_AMOUNT', 'FREE_SHIPPING', 'BUY_X_GET_Y'
);

-- Status de cupom
CREATE TYPE shared.coupon_status AS ENUM (
    'DRAFT', 'SCHEDULED', 'ACTIVE', 'PAUSED', 'EXPIRED', 'DEPLETED'
);

-- Escopo de cupom
CREATE TYPE shared.coupon_scope AS ENUM (
    'ALL', 'CATEGORIES', 'PRODUCTS', 'FIRST_PURCHASE', 'SPECIFIC_USERS'
);

-- ========================================
-- PARTE 2: MÓDULO DE USUÁRIOS (users)
-- ========================================

-- ========================================
-- 2.1 ASP.NET CORE IDENTITY
-- ========================================
-- NOTA: Estas tabelas são gerenciadas pelo Entity Framework Core Migrations.
-- Ver: src/modules/users/Users.Infrastructure/Persistence/Migrations/

/*
-- Tabela principal de usuários (gerenciada pelo ASP.NET Identity)
CREATE TABLE users.asp_net_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_name VARCHAR(256),
    normalized_user_name VARCHAR(256),
    email VARCHAR(256),
    normalized_email VARCHAR(256),
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    password_hash TEXT,
    security_stamp TEXT,
    concurrency_stamp TEXT,
    phone_number VARCHAR(50),
    phone_number_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    two_factor_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    lockout_end TIMESTAMPTZ,
    lockout_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    access_failed_count INT NOT NULL DEFAULT 0,

    -- Campos customizados
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Roles (papéis/perfis de acesso)
CREATE TABLE users.asp_net_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(256),
    normalized_name VARCHAR(256),
    concurrency_stamp TEXT
);

-- Relacionamento usuário-role (many-to-many)
CREATE TABLE users.asp_net_user_roles (
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES users.asp_net_roles(id) ON DELETE CASCADE,

    PRIMARY KEY (user_id, role_id)
);

-- Claims de usuário (permissões específicas por usuário)
CREATE TABLE users.asp_net_user_claims (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    claim_type VARCHAR(255),
    claim_value TEXT
);

-- Claims de role (permissões por papel)
CREATE TABLE users.asp_net_role_claims (
    id SERIAL PRIMARY KEY,
    role_id UUID NOT NULL REFERENCES users.asp_net_roles(id) ON DELETE CASCADE,
    claim_type VARCHAR(255),
    claim_value TEXT
);

-- Logins externos (Google, Facebook, etc.)
CREATE TABLE users.asp_net_user_logins (
    login_provider VARCHAR(128) NOT NULL,
    provider_key VARCHAR(128) NOT NULL,
    provider_display_name VARCHAR(255),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,

    PRIMARY KEY (login_provider, provider_key)
);

-- Tokens de usuário (recuperação de senha, confirmação de email, etc.)
CREATE TABLE users.asp_net_user_tokens (
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    login_provider VARCHAR(128) NOT NULL,
    name VARCHAR(128) NOT NULL,
    value TEXT,

    PRIMARY KEY (user_id, login_provider, name)
);
*/

-- ========================================
-- 2.2 TABELAS CUSTOMIZADAS DE USUÁRIO
-- ========================================
-- NOTA: Estas tabelas são gerenciadas pelo Entity Framework Core Migrations.
-- Ver: src/modules/users/Users.Infrastructure/Persistence/Migrations/

/*
-- Perfil estendido do usuário
CREATE TABLE users.profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL UNIQUE REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    -- Dados pessoais
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    display_name VARCHAR(100),
    avatar_url TEXT,
    birth_date DATE,
    gender VARCHAR(20),
    cpf VARCHAR(14),
    
    -- Preferências
    preferred_language VARCHAR(5) DEFAULT 'pt-BR',
    preferred_currency VARCHAR(3) DEFAULT 'BRL',
    newsletter_subscribed BOOLEAN DEFAULT FALSE,
    
    -- Termos
    accepted_terms_at TIMESTAMPTZ,
    accepted_privacy_at TIMESTAMPTZ,
    
    -- Controle
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_profiles_cpf_format CHECK (
        cpf IS NULL OR cpf ~ '^\d{3}\.\d{3}\.\d{3}-\d{2}$'
    )
);

-- Endereços
CREATE TABLE users.addresses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    -- Dados do endereço
    label VARCHAR(50),
    recipient_name VARCHAR(150),
    street VARCHAR(255) NOT NULL,
    number VARCHAR(20),
    complement VARCHAR(100),
    neighborhood VARCHAR(100),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(2) NOT NULL,
    postal_code VARCHAR(9) NOT NULL,
    country VARCHAR(2) NOT NULL DEFAULT 'BR',
    
    -- Coordenadas
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    ibge_code VARCHAR(7),
    
    -- Controle
    is_default BOOLEAN DEFAULT FALSE,
    is_billing_address BOOLEAN DEFAULT FALSE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_addresses_postal_code CHECK (postal_code ~ '^\d{5}-?\d{3}$'),
    CONSTRAINT chk_addresses_state CHECK (state ~ '^[A-Z]{2}$')
);

-- Histórico de login
CREATE TABLE users.login_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    login_provider VARCHAR(50) NOT NULL DEFAULT 'Local',
    ip_address VARCHAR(45),
    user_agent TEXT,
    country VARCHAR(2),
    city VARCHAR(100),
    device_type VARCHAR(20),
    device_info JSONB,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    failure_reason VARCHAR(100),
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Sessões ativas
CREATE TABLE users.sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    refresh_token_hash VARCHAR(512) NOT NULL,
    device_id VARCHAR(100),
    device_name VARCHAR(100),
    device_type VARCHAR(20),
    ip_address VARCHAR(45),
    country VARCHAR(2),
    city VARCHAR(100),
    is_current BOOLEAN DEFAULT FALSE,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    revoked_reason VARCHAR(100),
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_activity_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Notificações
CREATE TABLE users.notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    reference_type VARCHAR(50),
    reference_id UUID,
    action_url TEXT,
    read_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Preferências de notificação
CREATE TABLE users.notification_preferences (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL UNIQUE REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    email_enabled BOOLEAN DEFAULT TRUE,
    push_enabled BOOLEAN DEFAULT TRUE,
    sms_enabled BOOLEAN DEFAULT FALSE,
    order_updates BOOLEAN DEFAULT TRUE,
    promotions BOOLEAN DEFAULT TRUE,
    price_drops BOOLEAN DEFAULT TRUE,
    back_in_stock BOOLEAN DEFAULT TRUE,
    newsletter BOOLEAN DEFAULT FALSE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
*/

-- ========================================
-- PARTE 3: MÓDULO DE CATÁLOGO (catalog)
-- ========================================

-- Categorias (hierárquica)
CREATE TABLE catalog.categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    parent_id UUID REFERENCES catalog.categories(id) ON DELETE SET NULL,
    
    path TEXT,
    depth INT NOT NULL DEFAULT 0,
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(120) NOT NULL,
    description TEXT,
    image_url TEXT,
    
    -- SEO
    meta_title VARCHAR(70),
    meta_description VARCHAR(160),
    
    -- Controle
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INT DEFAULT 0,
    version INT NOT NULL DEFAULT 1,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_categories_name CHECK (char_length(trim(name)) > 0),
    CONSTRAINT chk_categories_no_self_parent CHECK (parent_id IS NULL OR parent_id <> id),
    CONSTRAINT chk_categories_depth CHECK (depth >= 0 AND depth <= 5)
);

-- Marcas (opcional - remover se não for usar)
CREATE TABLE catalog.brands (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(120) NOT NULL,
    description TEXT,
    logo_url TEXT,
    website_url TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INT DEFAULT 0,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    CONSTRAINT chk_brands_name CHECK (char_length(trim(name)) > 0)
);

-- Produtos
CREATE TABLE catalog.products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    category_id UUID REFERENCES catalog.categories(id) ON DELETE SET NULL,
    brand_id UUID REFERENCES catalog.brands(id) ON DELETE SET NULL,
    
    sku VARCHAR(100) UNIQUE NOT NULL,
    slug VARCHAR(200) NOT NULL,
    barcode VARCHAR(50),
    
    name VARCHAR(150) NOT NULL,
    short_description VARCHAR(500),
    description TEXT,
    
    -- Preços
    price DECIMAL(10, 2) NOT NULL,
    compare_at_price DECIMAL(10, 2),
    cost_price DECIMAL(10, 2),
    
    -- Estoque
    stock INT NOT NULL DEFAULT 0,
    reserved_stock INT NOT NULL DEFAULT 0,
    low_stock_threshold INT DEFAULT 10,
    
    -- Dimensões
    weight_grams INT,
    height_cm DECIMAL(6, 2),
    width_cm DECIMAL(6, 2),
    length_cm DECIMAL(6, 2),
    
    -- SEO
    meta_title VARCHAR(70),
    meta_description VARCHAR(160),
    
    -- Controle
    status shared.product_status NOT NULL DEFAULT 'DRAFT',
    is_featured BOOLEAN DEFAULT FALSE,
    is_digital BOOLEAN DEFAULT FALSE,
    requires_shipping BOOLEAN DEFAULT TRUE,
    
    -- Flexível
    attributes JSONB DEFAULT '{}',
    tags TEXT[],
    
    version INT NOT NULL DEFAULT 1,
    published_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_products_name CHECK (char_length(trim(name)) > 0),
    CONSTRAINT chk_products_price CHECK (price >= 0),
    CONSTRAINT chk_products_stock CHECK (stock >= 0),
    CONSTRAINT chk_products_reserved CHECK (reserved_stock >= 0),
    CONSTRAINT chk_products_compare_price CHECK (compare_at_price IS NULL OR compare_at_price > price)
);

-- Imagens de produtos
CREATE TABLE catalog.product_images (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    url TEXT NOT NULL,
    alt_text VARCHAR(255),
    url_thumbnail TEXT,
    url_medium TEXT,
    url_large TEXT,
    
    is_primary BOOLEAN DEFAULT FALSE,
    sort_order INT DEFAULT 0,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Movimentação de estoque
CREATE TABLE catalog.stock_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    movement_type shared.stock_movement_type NOT NULL,
    quantity INT NOT NULL,
    
    reference_type VARCHAR(50),
    reference_id UUID,
    
    stock_before INT NOT NULL,
    stock_after INT NOT NULL,
    
    reason TEXT,
    performed_by UUID,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Reservas de estoque
CREATE TABLE catalog.stock_reservations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    reference_type VARCHAR(50) NOT NULL,
    reference_id UUID NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    
    expires_at TIMESTAMPTZ NOT NULL,
    released_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_stock_reservation UNIQUE (product_id, reference_type, reference_id)
);

-- Avaliações de produtos
CREATE TABLE catalog.product_reviews (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    order_id UUID, -- FK será adicionada depois
    
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(200),
    comment TEXT,
    
    is_verified_purchase BOOLEAN DEFAULT FALSE,
    is_approved BOOLEAN DEFAULT FALSE,
    
    seller_response TEXT,
    seller_responded_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT uq_product_review_user UNIQUE (product_id, user_id)
);

-- Produtos favoritos (relacionamento users <-> catalog)
CREATE TABLE catalog.user_favorites (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    product_snapshot JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_user_favorite_product UNIQUE (user_id, product_id)
);

-- ========================================
-- PARTE 4: MÓDULO DE CARRINHO (cart)
-- ========================================

-- Carrinhos
CREATE TABLE cart.carts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    session_id VARCHAR(100),
    
    coupon_id UUID, -- FK será adicionada depois
    coupon_code VARCHAR(50),
    discount_amount DECIMAL(10, 2) DEFAULT 0,
    
    status shared.cart_status NOT NULL DEFAULT 'ACTIVE',
    ip_address VARCHAR(45),
    user_agent TEXT,
    
    version INT NOT NULL DEFAULT 1,
    expires_at TIMESTAMPTZ,
    converted_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_cart_owner CHECK (user_id IS NOT NULL OR session_id IS NOT NULL)
);

-- Itens do carrinho
CREATE TABLE cart.items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cart_id UUID NOT NULL REFERENCES cart.carts(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    product_snapshot JSONB NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10, 2) NOT NULL CHECK (unit_price >= 0),
    
    current_price DECIMAL(10, 2),
    price_changed_at TIMESTAMPTZ,
    
    stock_reserved BOOLEAN DEFAULT FALSE,
    stock_reservation_id UUID REFERENCES catalog.stock_reservations(id) ON DELETE SET NULL,
    
    added_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    removed_at TIMESTAMPTZ,
    
    CONSTRAINT uq_cart_item_product UNIQUE (cart_id, product_id)
);

-- Log de atividades do carrinho
CREATE TABLE cart.activity_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cart_id UUID NOT NULL REFERENCES cart.carts(id) ON DELETE CASCADE,
    
    action VARCHAR(50) NOT NULL,
    product_id UUID REFERENCES catalog.products(id) ON DELETE SET NULL,
    quantity_before INT,
    quantity_after INT,
    metadata JSONB,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Carrinhos salvos
CREATE TABLE cart.saved_carts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    name VARCHAR(100) NOT NULL DEFAULT 'Meu Carrinho Salvo',
    items JSONB NOT NULL,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========================================
-- PARTE 5: MÓDULO DE PEDIDOS (orders)
-- ========================================

-- Sequence para número do pedido
CREATE SEQUENCE IF NOT EXISTS orders.order_number_seq START 1;

-- Função para gerar número do pedido
CREATE OR REPLACE FUNCTION orders.generate_order_number()
RETURNS TEXT AS $$
DECLARE
    v_year TEXT;
    v_sequence INT;
BEGIN
    v_year := TO_CHAR(NOW(), 'YY');
    SELECT nextval('orders.order_number_seq') INTO v_sequence;
    RETURN v_year || '-' || LPAD(v_sequence::TEXT, 6, '0');
END;
$$ LANGUAGE plpgsql;

-- Pedidos
CREATE TABLE orders.orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_number VARCHAR(20) UNIQUE NOT NULL DEFAULT orders.generate_order_number(),

    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE RESTRICT,
    cart_id UUID REFERENCES cart.carts(id) ON DELETE SET NULL,
    coupon_id UUID, -- FK será adicionada depois
    
    -- Valores
    subtotal DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
    discount_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    shipping_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    tax_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    total DECIMAL(10, 2) NOT NULL CHECK (total >= 0),
    
    coupon_snapshot JSONB,
    status shared.order_status NOT NULL DEFAULT 'PENDING',
    
    -- Endereços (snapshot)
    shipping_address JSONB NOT NULL,
    billing_address JSONB,
    
    -- Entrega
    shipping_method shared.shipping_method NOT NULL DEFAULT 'STANDARD',
    shipping_carrier VARCHAR(100),
    tracking_code VARCHAR(100),
    tracking_url TEXT,
    estimated_delivery_at TIMESTAMPTZ,
    
    -- Pagamento
    payment_method shared.payment_method_type NOT NULL,
    
    -- Cancelamento
    cancellation_reason shared.cancellation_reason,
    cancellation_notes TEXT,
    cancelled_by UUID,
    
    -- Notas
    customer_notes TEXT,
    internal_notes TEXT,
    
    version INT NOT NULL DEFAULT 1,
    
    -- Timestamps de eventos
    paid_at TIMESTAMPTZ,
    shipped_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    refunded_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_orders_discount CHECK (discount_amount >= 0 AND discount_amount <= subtotal)
);

-- Itens do pedido
CREATE TABLE orders.items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE RESTRICT,
    
    product_snapshot JSONB NOT NULL,
    unit_price DECIMAL(10, 2) NOT NULL CHECK (unit_price >= 0),
    quantity INT NOT NULL CHECK (quantity > 0),
    discount_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    subtotal DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Histórico de status do pedido
CREATE TABLE orders.status_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    
    from_status shared.order_status,
    to_status shared.order_status NOT NULL,
    
    reason TEXT,
    changed_by UUID,
    metadata JSONB,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Eventos de rastreamento
CREATE TABLE orders.tracking_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    
    event_code VARCHAR(50) NOT NULL,
    event_description TEXT NOT NULL,
    location VARCHAR(200),
    city VARCHAR(100),
    state VARCHAR(2),
    
    occurred_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Notas fiscais
CREATE TABLE orders.invoices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    
    invoice_number VARCHAR(50) NOT NULL,
    invoice_key VARCHAR(50),
    invoice_series VARCHAR(10),
    
    pdf_url TEXT,
    xml_url TEXT,
    
    issued_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Reembolsos de pedido
CREATE TABLE orders.refunds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    
    amount DECIMAL(10, 2) NOT NULL CHECK (amount > 0),
    reason TEXT NOT NULL,
    refund_method VARCHAR(50),
    
    payment_id UUID, -- FK será adicionada depois
    gateway_refund_id VARCHAR(100),
    status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    
    processed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========================================
-- PARTE 6: MÓDULO DE PAGAMENTOS (payments)
-- ========================================

-- Métodos de pagamento salvos
CREATE TABLE payments.user_methods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    gateway_customer_id VARCHAR(100),
    gateway_payment_method_id VARCHAR(100) NOT NULL,
    gateway_name VARCHAR(50) NOT NULL,
    
    method_type shared.payment_method_type NOT NULL,
    
    -- Cartão (tokenizado)
    card_brand shared.card_brand,
    card_last_four VARCHAR(4),
    card_holder_name VARCHAR(150),
    card_expiration_month VARCHAR(2),
    card_expiration_year VARCHAR(4),
    
    -- Carteira digital
    wallet_type VARCHAR(50),
    wallet_email VARCHAR(255),
    
    is_default BOOLEAN DEFAULT FALSE,
    is_valid BOOLEAN DEFAULT TRUE,
    
    version INT NOT NULL DEFAULT 1,
    last_used_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_card_data CHECK (
        (method_type NOT IN ('CREDIT_CARD', 'DEBIT_CARD'))
        OR (card_last_four IS NOT NULL AND card_brand IS NOT NULL)
    )
);

-- Pagamentos
CREATE TABLE payments.payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE RESTRICT,
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE RESTRICT,
    
    idempotency_key VARCHAR(100) UNIQUE NOT NULL,
    
    amount DECIMAL(10, 2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    fee_amount DECIMAL(10, 2) DEFAULT 0,
    net_amount DECIMAL(10, 2),
    
    payment_method_type shared.payment_method_type NOT NULL,
    saved_payment_method_id UUID REFERENCES payments.user_methods(id) ON DELETE SET NULL,
    payment_method_snapshot JSONB,
    
    installments INT DEFAULT 1 CHECK (installments >= 1 AND installments <= 24),
    installment_amount DECIMAL(10, 2),
    
    -- Gateway
    gateway_name VARCHAR(50) NOT NULL,
    gateway_transaction_id VARCHAR(100),
    gateway_authorization_code VARCHAR(50),
    gateway_response JSONB,
    
    -- PIX
    pix_qr_code TEXT,
    pix_qr_code_url TEXT,
    pix_expiration_at TIMESTAMPTZ,
    
    -- Boleto
    boleto_url TEXT,
    boleto_barcode VARCHAR(50),
    boleto_expiration_at TIMESTAMPTZ,
    
    status shared.payment_status NOT NULL DEFAULT 'PENDING',
    
    -- Fraude
    fraud_score DECIMAL(5, 2),
    fraud_analysis JSONB,
    
    -- Erros
    error_code VARCHAR(50),
    error_message TEXT,
    
    version INT NOT NULL DEFAULT 1,
    
    -- Timestamps de eventos
    authorized_at TIMESTAMPTZ,
    captured_at TIMESTAMPTZ,
    failed_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    refunded_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Transações de pagamento
CREATE TABLE payments.transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL REFERENCES payments.payments(id) ON DELETE CASCADE,
    
    transaction_type shared.transaction_type NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    
    gateway_transaction_id VARCHAR(100),
    gateway_response JSONB,
    
    success BOOLEAN NOT NULL,
    error_code VARCHAR(50),
    error_message TEXT,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Reembolsos de pagamento
CREATE TABLE payments.refunds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL REFERENCES payments.payments(id) ON DELETE CASCADE,
    
    idempotency_key VARCHAR(100) UNIQUE NOT NULL,
    amount DECIMAL(10, 2) NOT NULL CHECK (amount > 0),
    reason TEXT NOT NULL,
    
    gateway_refund_id VARCHAR(100),
    gateway_response JSONB,
    
    status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    order_refund_id UUID REFERENCES orders.refunds(id) ON DELETE SET NULL,
    
    processed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Chargebacks
CREATE TABLE payments.chargebacks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL REFERENCES payments.payments(id) ON DELETE CASCADE,
    
    gateway_chargeback_id VARCHAR(100),
    reason_code VARCHAR(50),
    reason_description TEXT,
    amount DECIMAL(10, 2) NOT NULL,
    
    evidence_submitted BOOLEAN DEFAULT FALSE,
    evidence_due_at TIMESTAMPTZ,
    
    status VARCHAR(50) NOT NULL DEFAULT 'OPEN',
    result VARCHAR(50),
    
    opened_at TIMESTAMPTZ NOT NULL,
    resolved_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Webhooks recebidos
CREATE TABLE payments.webhooks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    gateway_name VARCHAR(50) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    
    payload JSONB NOT NULL,
    headers JSONB,
    
    processed BOOLEAN DEFAULT FALSE,
    processed_at TIMESTAMPTZ,
    error_message TEXT,
    
    payment_id UUID REFERENCES payments.payments(id) ON DELETE SET NULL,
    
    received_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========================================
-- PARTE 7: MÓDULO DE CUPONS (coupons)
-- ========================================

-- Cupons
CREATE TABLE coupons.coupons (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    code CITEXT UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    
    discount_type shared.coupon_type NOT NULL,
    discount_value DECIMAL(10, 2) NOT NULL CHECK (discount_value > 0),
    max_discount_amount DECIMAL(10, 2),
    
    scope shared.coupon_scope NOT NULL DEFAULT 'ALL',
    
    min_purchase_amount DECIMAL(10, 2) DEFAULT 0,
    min_items_quantity INT DEFAULT 1,
    
    -- Buy X Get Y
    buy_quantity INT,
    get_quantity INT,
    
    valid_from TIMESTAMPTZ NOT NULL,
    valid_until TIMESTAMPTZ NOT NULL,
    
    max_uses INT,
    current_uses INT NOT NULL DEFAULT 0,
    max_uses_per_user INT DEFAULT 1,
    
    is_stackable BOOLEAN DEFAULT FALSE,
    status shared.coupon_status NOT NULL DEFAULT 'DRAFT',
    
    created_by UUID REFERENCES users.asp_net_users(id) ON DELETE SET NULL,
    metadata JSONB DEFAULT '{}',
    
    version INT NOT NULL DEFAULT 1,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    CONSTRAINT chk_coupon_code_format CHECK (char_length(code) >= 3 AND char_length(code) <= 50),
    CONSTRAINT chk_coupon_percentage CHECK (discount_type != 'PERCENTAGE' OR discount_value <= 100),
    CONSTRAINT chk_coupon_validity CHECK (valid_until > valid_from),
    CONSTRAINT chk_coupon_max_uses CHECK (max_uses IS NULL OR max_uses > 0),
    CONSTRAINT chk_coupon_buy_get CHECK (
        discount_type != 'BUY_X_GET_Y' 
        OR (buy_quantity IS NOT NULL AND get_quantity IS NOT NULL AND buy_quantity > 0 AND get_quantity > 0)
    )
);

-- Categorias elegíveis
CREATE TABLE coupons.eligible_categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coupon_id UUID NOT NULL REFERENCES coupons.coupons(id) ON DELETE CASCADE,
    category_id UUID NOT NULL REFERENCES catalog.categories(id) ON DELETE CASCADE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_coupon_category UNIQUE (coupon_id, category_id)
);

-- Produtos elegíveis
CREATE TABLE coupons.eligible_products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coupon_id UUID NOT NULL REFERENCES coupons.coupons(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_coupon_product UNIQUE (coupon_id, product_id)
);

-- Usuários elegíveis
CREATE TABLE coupons.eligible_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coupon_id UUID NOT NULL REFERENCES coupons.coupons(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users.asp_net_users(id) ON DELETE CASCADE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_coupon_user UNIQUE (coupon_id, user_id)
);

-- Uso de cupons
CREATE TABLE coupons.usages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coupon_id UUID NOT NULL REFERENCES coupons.coupons(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users.asp_net_users(id) ON DELETE SET NULL,
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    
    discount_amount DECIMAL(10, 2) NOT NULL CHECK (discount_amount >= 0),
    order_subtotal DECIMAL(10, 2) NOT NULL,
    
    used_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Reservas de cupom
CREATE TABLE coupons.reservations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    coupon_id UUID NOT NULL REFERENCES coupons.coupons(id) ON DELETE CASCADE,
    cart_id UUID NOT NULL REFERENCES cart.carts(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users.asp_net_users(id) ON DELETE SET NULL,
    
    discount_amount DECIMAL(10, 2) NOT NULL,
    
    expires_at TIMESTAMPTZ NOT NULL,
    released_at TIMESTAMPTZ,
    converted_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_coupon_reservation UNIQUE (coupon_id, cart_id)
);

-- ========================================
-- PARTE 8: INFRAESTRUTURA DE EVENTOS E AUDITORIA
-- ========================================

-- Outbox unificado para eventos de domínio
CREATE TABLE shared.domain_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    module VARCHAR(50) NOT NULL, -- users, catalog, cart, orders, payments, coupons
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    error_message TEXT,
    retry_count INT DEFAULT 0
);

-- Inbox para idempotência de eventos
CREATE TABLE shared.processed_events (
    id UUID PRIMARY KEY,
    event_type VARCHAR(100) NOT NULL,
    module VARCHAR(50) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Audit log unificado
CREATE TABLE shared.audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    module VARCHAR(50) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    
    old_values JSONB,
    new_values JSONB,
    
    user_id UUID,
    ip_address VARCHAR(45),
    user_agent TEXT,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========================================
-- PARTE 9: FOREIGN KEYS TARDIAS (dependências cruzadas)
-- ========================================

-- Cart -> Coupon
ALTER TABLE cart.carts 
    ADD CONSTRAINT fk_carts_coupon 
    FOREIGN KEY (coupon_id) REFERENCES coupons.coupons(id) ON DELETE SET NULL;

-- Orders -> Coupon
ALTER TABLE orders.orders 
    ADD CONSTRAINT fk_orders_coupon 
    FOREIGN KEY (coupon_id) REFERENCES coupons.coupons(id) ON DELETE SET NULL;

-- Orders.refunds -> Payment
ALTER TABLE orders.refunds 
    ADD CONSTRAINT fk_order_refunds_payment 
    FOREIGN KEY (payment_id) REFERENCES payments.payments(id) ON DELETE SET NULL;

-- Product Reviews -> Order (para verificar compra)
ALTER TABLE catalog.product_reviews
    ADD CONSTRAINT fk_reviews_order
    FOREIGN KEY (order_id) REFERENCES orders.orders(id) ON DELETE SET NULL;

-- Orders -> User (validação tardia para garantir ordem de criação)
-- Já foi adicionada diretamente na tabela orders (linha 690)

-- ========================================
-- PARTE 10: TRIGGERS
-- ========================================

-- Users - Identity and custom tables (managed by EF Core Migrations)
/*
-- Trigger para atualizar updated_at em asp_net_users
CREATE TRIGGER trg_asp_net_users_updated_at
    BEFORE UPDATE ON users.asp_net_users
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

-- Users - Tabelas customizadas
CREATE TRIGGER trg_profiles_updated_at
    BEFORE UPDATE ON users.profiles
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_profiles_version
    BEFORE UPDATE ON users.profiles
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

CREATE TRIGGER trg_addresses_updated_at
    BEFORE UPDATE ON users.addresses
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_notification_prefs_updated_at
    BEFORE UPDATE ON users.notification_preferences
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();
*/

-- Catalog - Brands
CREATE TRIGGER trg_brands_updated_at
    BEFORE UPDATE ON catalog.brands
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

-- Catalog - Categories
CREATE TRIGGER trg_categories_updated_at
    BEFORE UPDATE ON catalog.categories
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_categories_version
    BEFORE UPDATE ON catalog.categories
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON catalog.products
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_products_version
    BEFORE UPDATE ON catalog.products
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

CREATE TRIGGER trg_reviews_updated_at
    BEFORE UPDATE ON catalog.product_reviews
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

-- Cart
CREATE TRIGGER trg_carts_updated_at
    BEFORE UPDATE ON cart.carts
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_carts_version
    BEFORE UPDATE ON cart.carts
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

CREATE TRIGGER trg_cart_items_updated_at
    BEFORE UPDATE ON cart.items
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_saved_carts_updated_at
    BEFORE UPDATE ON cart.saved_carts
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

-- Orders
CREATE TRIGGER trg_orders_updated_at
    BEFORE UPDATE ON orders.orders
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_orders_version
    BEFORE UPDATE ON orders.orders
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

-- Trigger para criar histórico de status automaticamente
CREATE OR REPLACE FUNCTION orders.trigger_order_status_history()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status IS DISTINCT FROM NEW.status THEN
        INSERT INTO orders.status_history (order_id, from_status, to_status)
        VALUES (NEW.id, OLD.status, NEW.status);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_orders_status_history
    AFTER UPDATE ON orders.orders
    FOR EACH ROW EXECUTE FUNCTION orders.trigger_order_status_history();

-- Payments
CREATE TRIGGER trg_user_methods_updated_at
    BEFORE UPDATE ON payments.user_methods
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_user_methods_version
    BEFORE UPDATE ON payments.user_methods
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

CREATE TRIGGER trg_payments_updated_at
    BEFORE UPDATE ON payments.payments
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_payments_version
    BEFORE UPDATE ON payments.payments
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

-- Coupons
CREATE TRIGGER trg_coupons_updated_at
    BEFORE UPDATE ON coupons.coupons
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();

CREATE TRIGGER trg_coupons_version
    BEFORE UPDATE ON coupons.coupons
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();

-- Trigger para incrementar current_uses
CREATE OR REPLACE FUNCTION coupons.trigger_increment_coupon_usage()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE coupons.coupons 
    SET current_uses = current_uses + 1 
    WHERE id = NEW.coupon_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_coupon_usage_increment
    AFTER INSERT ON coupons.usages
    FOR EACH ROW EXECUTE FUNCTION coupons.trigger_increment_coupon_usage();

-- Trigger para atualizar status quando limite é atingido
CREATE OR REPLACE FUNCTION coupons.trigger_check_coupon_depleted()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.max_uses IS NOT NULL AND NEW.current_uses >= NEW.max_uses THEN
        NEW.status := 'DEPLETED';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_coupon_check_depleted
    BEFORE UPDATE ON coupons.coupons
    FOR EACH ROW EXECUTE FUNCTION coupons.trigger_check_coupon_depleted();

-- ========================================
-- PARTE 11: INDEXES
-- ========================================

-- Users - ASP.NET Identity and custom tables (managed by EF Core Migrations)
/*
CREATE UNIQUE INDEX user_name_index ON users.asp_net_users(normalized_user_name) WHERE normalized_user_name IS NOT NULL;
CREATE INDEX email_index ON users.asp_net_users(normalized_email);
CREATE UNIQUE INDEX role_name_index ON users.asp_net_roles(normalized_name) WHERE normalized_name IS NOT NULL;
CREATE INDEX ix_asp_net_user_roles_role_id ON users.asp_net_user_roles(role_id);
CREATE INDEX ix_asp_net_user_claims_user_id ON users.asp_net_user_claims(user_id);
CREATE INDEX ix_asp_net_role_claims_role_id ON users.asp_net_role_claims(role_id);
CREATE INDEX ix_asp_net_user_logins_user_id ON users.asp_net_user_logins(user_id);

-- Users - Tabelas customizadas
CREATE INDEX idx_profiles_user_id ON users.profiles(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_profiles_cpf ON users.profiles(cpf) WHERE cpf IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_addresses_user_id ON users.addresses(user_id) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX uq_addresses_default_per_user ON users.addresses(user_id) 
    WHERE is_default = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_login_history_user_id ON users.login_history(user_id);
CREATE INDEX idx_login_history_created_at ON users.login_history(created_at DESC);
CREATE INDEX idx_sessions_user_id ON users.sessions(user_id) WHERE revoked_at IS NULL;
CREATE INDEX idx_sessions_expires ON users.sessions(expires_at) WHERE revoked_at IS NULL;
CREATE UNIQUE INDEX uq_sessions_refresh_token ON users.sessions(refresh_token_hash);
CREATE INDEX idx_notifications_user_id ON users.notifications(user_id);
CREATE INDEX idx_notifications_unread ON users.notifications(user_id, created_at DESC) WHERE read_at IS NULL;
*/

-- Catalog - Brands
CREATE UNIQUE INDEX uq_brands_slug ON catalog.brands(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_brands_name ON catalog.brands(name) WHERE deleted_at IS NULL;
CREATE INDEX idx_brands_active ON catalog.brands(is_active) WHERE is_active = TRUE AND deleted_at IS NULL;

-- Catalog - Categories
CREATE UNIQUE INDEX uq_categories_slug ON catalog.categories(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_categories_parent_id ON catalog.categories(parent_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_categories_path ON catalog.categories(path) WHERE deleted_at IS NULL;
CREATE INDEX idx_categories_name_trgm ON catalog.categories USING GIN (name gin_trgm_ops);
CREATE UNIQUE INDEX uq_categories_parent_name ON catalog.categories(parent_id, LOWER(name)) WHERE deleted_at IS NULL;

-- Catalog - Products
CREATE UNIQUE INDEX uq_products_slug ON catalog.products(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_brand_id ON catalog.products(brand_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_category_id ON catalog.products(category_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_status ON catalog.products(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_active ON catalog.products(category_id, status) WHERE status = 'ACTIVE' AND deleted_at IS NULL;
CREATE INDEX idx_products_featured ON catalog.products(is_featured) WHERE is_featured = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_products_price ON catalog.products(price) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_created_at ON catalog.products(created_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_name_trgm ON catalog.products USING GIN (name gin_trgm_ops);
CREATE INDEX idx_products_tags ON catalog.products USING GIN (tags);
CREATE INDEX idx_products_attributes ON catalog.products USING GIN (attributes);
CREATE INDEX idx_products_low_stock ON catalog.products(stock) WHERE stock <= low_stock_threshold AND status = 'ACTIVE';

CREATE INDEX idx_product_images_product_id ON catalog.product_images(product_id);
CREATE UNIQUE INDEX uq_product_images_primary ON catalog.product_images(product_id) WHERE is_primary = TRUE;

CREATE INDEX idx_stock_movements_product_id ON catalog.stock_movements(product_id);
CREATE INDEX idx_stock_movements_created_at ON catalog.stock_movements(created_at DESC);

CREATE INDEX idx_stock_reservations_product_id ON catalog.stock_reservations(product_id) WHERE released_at IS NULL;
CREATE INDEX idx_stock_reservations_expires ON catalog.stock_reservations(expires_at) WHERE released_at IS NULL;

CREATE INDEX idx_product_reviews_product_id ON catalog.product_reviews(product_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_product_reviews_user_id ON catalog.product_reviews(user_id);
CREATE INDEX idx_product_reviews_rating ON catalog.product_reviews(product_id, rating) WHERE is_approved = TRUE AND deleted_at IS NULL;

CREATE INDEX idx_user_favorites_user_id ON catalog.user_favorites(user_id);
CREATE INDEX idx_user_favorites_product_id ON catalog.user_favorites(product_id);

-- Cart
CREATE UNIQUE INDEX uq_carts_user_active ON cart.carts(user_id) WHERE user_id IS NOT NULL AND status = 'ACTIVE';
CREATE UNIQUE INDEX uq_carts_session_active ON cart.carts(session_id) WHERE session_id IS NOT NULL AND status = 'ACTIVE';
CREATE INDEX idx_carts_status ON cart.carts(status);
CREATE INDEX idx_carts_expires_at ON cart.carts(expires_at) WHERE status = 'ACTIVE';
CREATE INDEX idx_carts_abandoned ON cart.carts(updated_at) WHERE status = 'ACTIVE';

CREATE INDEX idx_cart_items_cart_id ON cart.items(cart_id) WHERE removed_at IS NULL;
CREATE INDEX idx_cart_items_product_id ON cart.items(product_id);
CREATE INDEX idx_cart_items_price_changed ON cart.items(cart_id) WHERE price_changed_at IS NOT NULL AND removed_at IS NULL;

CREATE INDEX idx_cart_activity_cart_id ON cart.activity_log(cart_id);
CREATE INDEX idx_saved_carts_user_id ON cart.saved_carts(user_id);

-- Orders
CREATE INDEX idx_orders_user_id ON orders.orders(user_id);
CREATE INDEX idx_orders_status ON orders.orders(status);
CREATE INDEX idx_orders_created_at ON orders.orders(created_at DESC);
CREATE INDEX idx_orders_tracking_code ON orders.orders(tracking_code) WHERE tracking_code IS NOT NULL;
CREATE INDEX idx_orders_user_status ON orders.orders(user_id, status);
CREATE INDEX idx_orders_pending ON orders.orders(created_at) WHERE status = 'PENDING';

CREATE INDEX idx_order_items_order_id ON orders.items(order_id);
CREATE INDEX idx_order_items_product_id ON orders.items(product_id);

CREATE INDEX idx_order_status_history_order_id ON orders.status_history(order_id);
CREATE INDEX idx_tracking_events_order_id ON orders.tracking_events(order_id);
CREATE INDEX idx_order_invoices_order_id ON orders.invoices(order_id);
CREATE UNIQUE INDEX uq_order_invoices_number ON orders.invoices(invoice_number);
CREATE INDEX idx_order_refunds_order_id ON orders.refunds(order_id);

-- Payments
CREATE INDEX idx_user_methods_user_id ON payments.user_methods(user_id) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX uq_user_methods_default ON payments.user_methods(user_id) 
    WHERE is_default = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_user_methods_gateway ON payments.user_methods(gateway_name, gateway_payment_method_id);

CREATE INDEX idx_payments_order_id ON payments.payments(order_id);
CREATE INDEX idx_payments_user_id ON payments.payments(user_id);
CREATE INDEX idx_payments_status ON payments.payments(status);
CREATE INDEX idx_payments_created_at ON payments.payments(created_at DESC);
CREATE INDEX idx_payments_gateway ON payments.payments(gateway_name, gateway_transaction_id);
CREATE INDEX idx_payments_pending ON payments.payments(created_at) WHERE status = 'PENDING';
CREATE INDEX idx_payments_pix_expiring ON payments.payments(pix_expiration_at) 
    WHERE payment_method_type = 'PIX' AND status = 'PENDING';
CREATE INDEX idx_payments_boleto_expiring ON payments.payments(boleto_expiration_at) 
    WHERE payment_method_type = 'BOLETO' AND status = 'PENDING';

CREATE INDEX idx_payment_transactions_payment_id ON payments.transactions(payment_id);
CREATE INDEX idx_payment_refunds_payment_id ON payments.refunds(payment_id);
CREATE INDEX idx_payment_chargebacks_payment_id ON payments.chargebacks(payment_id);
CREATE INDEX idx_payment_chargebacks_evidence_due ON payments.chargebacks(evidence_due_at) WHERE status = 'OPEN';
CREATE INDEX idx_payment_webhooks_unprocessed ON payments.webhooks(received_at) WHERE processed = FALSE;

-- Coupons
CREATE INDEX idx_coupons_code ON coupons.coupons(code) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupons_status ON coupons.coupons(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupons_valid_period ON coupons.coupons(valid_from, valid_until) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupons_active ON coupons.coupons(status) WHERE status = 'ACTIVE' AND deleted_at IS NULL;
CREATE INDEX idx_coupons_expiring ON coupons.coupons(valid_until) WHERE status = 'ACTIVE' AND deleted_at IS NULL;

CREATE INDEX idx_coupon_categories_coupon_id ON coupons.eligible_categories(coupon_id);
CREATE INDEX idx_coupon_products_coupon_id ON coupons.eligible_products(coupon_id);
CREATE INDEX idx_coupon_users_coupon_id ON coupons.eligible_users(coupon_id);

CREATE INDEX idx_coupon_usages_coupon_id ON coupons.usages(coupon_id);
CREATE INDEX idx_coupon_usages_user_id ON coupons.usages(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX idx_coupon_usages_order_id ON coupons.usages(order_id);

CREATE INDEX idx_coupon_reservations_coupon_id ON coupons.reservations(coupon_id) 
    WHERE released_at IS NULL AND converted_at IS NULL;
CREATE INDEX idx_coupon_reservations_cart_id ON coupons.reservations(cart_id);
CREATE INDEX idx_coupon_reservations_expires ON coupons.reservations(expires_at) 
    WHERE released_at IS NULL AND converted_at IS NULL;

-- Shared
CREATE INDEX idx_domain_events_unprocessed ON shared.domain_events(created_at) WHERE processed_at IS NULL;
CREATE INDEX idx_domain_events_module ON shared.domain_events(module, aggregate_type, aggregate_id);
CREATE INDEX idx_audit_logs_entity ON shared.audit_logs(module, entity_type, entity_id);
CREATE INDEX idx_audit_logs_user ON shared.audit_logs(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX idx_audit_logs_created ON shared.audit_logs(created_at DESC);

-- ========================================
-- PARTE 12: VIEWS
-- ========================================

-- Users views (managed by EF Core Migrations - tables created via migration)
/*
-- View de sessões ativas por usuário
CREATE VIEW users.v_active_sessions AS
SELECT 
    user_id,
    COUNT(*) AS active_sessions,
    MAX(last_activity_at) AS last_activity,
    ARRAY_AGG(DISTINCT device_type) FILTER (WHERE device_type IS NOT NULL) AS device_types
FROM users.sessions
WHERE revoked_at IS NULL AND expires_at > NOW()
GROUP BY user_id;

-- View de notificações não lidas
CREATE VIEW users.v_unread_notifications AS
SELECT 
    user_id,
    COUNT(*) AS unread_count,
    MIN(created_at) AS oldest_unread,
    MAX(created_at) AS newest_unread
FROM users.notifications
WHERE read_at IS NULL
GROUP BY user_id;
*/

-- View materializada para estatísticas de produtos
CREATE MATERIALIZED VIEW catalog.mv_product_stats AS
SELECT 
    p.id AS product_id,
    p.name,
    p.price,
    p.stock,
    COALESCE(AVG(r.rating), 0) AS avg_rating,
    COUNT(r.id) AS review_count,
    COUNT(r.id) FILTER (WHERE r.rating = 5) AS five_star_count,
    COUNT(r.id) FILTER (WHERE r.rating = 4) AS four_star_count,
    COUNT(r.id) FILTER (WHERE r.rating = 3) AS three_star_count,
    COUNT(r.id) FILTER (WHERE r.rating = 2) AS two_star_count,
    COUNT(r.id) FILTER (WHERE r.rating = 1) AS one_star_count
FROM catalog.products p
LEFT JOIN catalog.product_reviews r ON r.product_id = p.id AND r.is_approved = TRUE AND r.deleted_at IS NULL
WHERE p.deleted_at IS NULL
GROUP BY p.id, p.name, p.price, p.stock;

CREATE UNIQUE INDEX idx_mv_product_stats_id ON catalog.mv_product_stats(product_id);

-- View para carrinhos ativos com totais
CREATE VIEW cart.v_active_carts AS
SELECT 
    c.id,
    c.user_id,
    c.session_id,
    c.coupon_code,
    c.discount_amount,
    c.status,
    c.created_at,
    c.updated_at,
    (SELECT COUNT(*) FROM cart.items ci WHERE ci.cart_id = c.id AND ci.removed_at IS NULL) AS item_count,
    (SELECT COALESCE(SUM(ci.quantity), 0) FROM cart.items ci WHERE ci.cart_id = c.id AND ci.removed_at IS NULL) AS total_items,
    (SELECT COALESCE(SUM(ci.unit_price * ci.quantity), 0) FROM cart.items ci WHERE ci.cart_id = c.id AND ci.removed_at IS NULL) AS subtotal
FROM cart.carts c
WHERE c.status = 'ACTIVE';

-- View para detecção de carrinhos abandonados
CREATE VIEW cart.v_abandoned_carts AS
SELECT 
    c.id,
    c.user_id,
    c.session_id,
    c.updated_at,
    NOW() - c.updated_at AS time_since_update,
    (SELECT COUNT(*) FROM cart.items ci WHERE ci.cart_id = c.id AND ci.removed_at IS NULL) AS item_count,
    (SELECT COALESCE(SUM(ci.unit_price * ci.quantity), 0) FROM cart.items ci WHERE ci.cart_id = c.id AND ci.removed_at IS NULL) AS cart_value
FROM cart.carts c
WHERE c.status = 'ACTIVE'
AND c.updated_at < NOW() - INTERVAL '1 hour';

-- View de resumo de pedidos por usuário
CREATE VIEW orders.v_user_order_summary AS
SELECT 
    user_id,
    COUNT(*) AS total_orders,
    COUNT(*) FILTER (WHERE status = 'DELIVERED') AS completed_orders,
    COUNT(*) FILTER (WHERE status = 'CANCELLED') AS cancelled_orders,
    SUM(total) AS total_spent,
    AVG(total) AS avg_order_value,
    MAX(created_at) AS last_order_at
FROM orders.orders
GROUP BY user_id;

-- View de pedidos pendentes de ação
CREATE VIEW orders.v_orders_pending_action AS
SELECT 
    o.*,
    CASE 
        WHEN o.status = 'PENDING' AND o.created_at < NOW() - INTERVAL '30 minutes' 
            THEN 'PAYMENT_TIMEOUT_RISK'
        WHEN o.status = 'PAID' AND o.created_at < NOW() - INTERVAL '2 hours' 
            THEN 'NEEDS_PREPARATION'
        WHEN o.status = 'SHIPPED' AND o.shipped_at < NOW() - INTERVAL '7 days' 
            THEN 'DELIVERY_DELAYED'
        ELSE 'OK'
    END AS alert_status
FROM orders.orders o
WHERE o.status NOT IN ('DELIVERED', 'CANCELLED', 'REFUNDED');

-- View de pagamentos pendentes de expiração
CREATE VIEW payments.v_expiring_payments AS
SELECT 
    p.*,
    CASE 
        WHEN p.payment_method_type = 'PIX' THEN p.pix_expiration_at
        WHEN p.payment_method_type = 'BOLETO' THEN p.boleto_expiration_at
        ELSE p.expires_at
    END AS expiration_date,
    CASE 
        WHEN p.payment_method_type = 'PIX' THEN p.pix_expiration_at - NOW()
        WHEN p.payment_method_type = 'BOLETO' THEN p.boleto_expiration_at - NOW()
        ELSE p.expires_at - NOW()
    END AS time_until_expiration
FROM payments.payments p
WHERE p.status = 'PENDING'
AND (
    (p.payment_method_type = 'PIX' AND p.pix_expiration_at IS NOT NULL)
    OR (p.payment_method_type = 'BOLETO' AND p.boleto_expiration_at IS NOT NULL)
    OR p.expires_at IS NOT NULL
);

-- View de métricas de pagamento
CREATE VIEW payments.v_payment_metrics AS
SELECT 
    DATE_TRUNC('day', created_at) AS date,
    COUNT(*) AS total_payments,
    COUNT(*) FILTER (WHERE status = 'CAPTURED') AS successful_payments,
    COUNT(*) FILTER (WHERE status = 'FAILED') AS failed_payments,
    SUM(amount) FILTER (WHERE status = 'CAPTURED') AS total_captured,
    AVG(amount) FILTER (WHERE status = 'CAPTURED') AS avg_payment_amount,
    payment_method_type,
    gateway_name
FROM payments.payments
GROUP BY DATE_TRUNC('day', created_at), payment_method_type, gateway_name;

-- View de cupons ativos e válidos
CREATE VIEW coupons.v_active_coupons AS
SELECT 
    c.*,
    c.max_uses - c.current_uses AS remaining_uses,
    CASE 
        WHEN c.max_uses IS NOT NULL THEN 
            ROUND((c.current_uses::DECIMAL / c.max_uses) * 100, 2)
        ELSE NULL
    END AS usage_percentage
FROM coupons.coupons c
WHERE c.status = 'ACTIVE'
AND c.deleted_at IS NULL
AND NOW() BETWEEN c.valid_from AND c.valid_until
AND (c.max_uses IS NULL OR c.current_uses < c.max_uses);

-- View de métricas de cupons
CREATE VIEW coupons.v_coupon_metrics AS
SELECT 
    c.id,
    c.code,
    c.name,
    c.discount_type,
    c.discount_value,
    c.status,
    c.current_uses,
    c.max_uses,
    COALESCE(SUM(cu.discount_amount), 0) AS total_discount_given,
    COALESCE(SUM(cu.order_subtotal), 0) AS total_order_value,
    COUNT(DISTINCT cu.user_id) AS unique_users,
    AVG(cu.discount_amount) AS avg_discount_per_use,
    MIN(cu.used_at) AS first_used_at,
    MAX(cu.used_at) AS last_used_at
FROM coupons.coupons c
LEFT JOIN coupons.usages cu ON cu.coupon_id = c.id
WHERE c.deleted_at IS NULL
GROUP BY c.id, c.code, c.name, c.discount_type, c.discount_value, c.status, c.current_uses, c.max_uses;

-- ========================================
-- PARTE 13: FUNÇÕES DE DOMÍNIO
-- ========================================

-- Função para validar uso do cupom
CREATE OR REPLACE FUNCTION coupons.validate_coupon_usage(
    p_coupon_id UUID,
    p_user_id UUID,
    p_cart_subtotal DECIMAL
)
RETURNS TABLE (
    is_valid BOOLEAN,
    error_code VARCHAR(50),
    error_message TEXT,
    discount_amount DECIMAL(10, 2)
) AS $$
DECLARE
    v_coupon RECORD;
    v_user_usage_count INT;
BEGIN
    SELECT * INTO v_coupon FROM coupons.coupons WHERE id = p_coupon_id AND deleted_at IS NULL;
    
    IF v_coupon IS NULL THEN
        RETURN QUERY SELECT FALSE, 'COUPON_NOT_FOUND'::VARCHAR(50), 'Cupom não encontrado'::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF v_coupon.status != 'ACTIVE' THEN
        RETURN QUERY SELECT FALSE, 'COUPON_INACTIVE'::VARCHAR(50), 'Cupom inativo'::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF NOW() < v_coupon.valid_from THEN
        RETURN QUERY SELECT FALSE, 'COUPON_NOT_STARTED'::VARCHAR(50), 'Cupom ainda não está válido'::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF NOW() > v_coupon.valid_until THEN
        RETURN QUERY SELECT FALSE, 'COUPON_EXPIRED'::VARCHAR(50), 'Cupom expirado'::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF v_coupon.max_uses IS NOT NULL AND v_coupon.current_uses >= v_coupon.max_uses THEN
        RETURN QUERY SELECT FALSE, 'COUPON_LIMIT_REACHED'::VARCHAR(50), 'Limite de uso do cupom atingido'::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF p_cart_subtotal < v_coupon.min_purchase_amount THEN
        RETURN QUERY SELECT FALSE, 'MIN_PURCHASE_NOT_MET'::VARCHAR(50), 
            FORMAT('Compra mínima de R$ %s não atingida', v_coupon.min_purchase_amount)::TEXT, 0::DECIMAL(10,2);
        RETURN;
    END IF;
    
    IF v_coupon.max_uses_per_user IS NOT NULL AND p_user_id IS NOT NULL THEN
        SELECT COUNT(*) INTO v_user_usage_count 
        FROM coupons.usages 
        WHERE coupon_id = p_coupon_id AND user_id = p_user_id;
        
        IF v_user_usage_count >= v_coupon.max_uses_per_user THEN
            RETURN QUERY SELECT FALSE, 'USER_LIMIT_REACHED'::VARCHAR(50), 
                'Você já utilizou este cupom o máximo de vezes permitido'::TEXT, 0::DECIMAL(10,2);
            RETURN;
        END IF;
    END IF;
    
    IF v_coupon.discount_type = 'PERCENTAGE' THEN
        RETURN QUERY SELECT TRUE, NULL::VARCHAR(50), NULL::TEXT, 
            LEAST(p_cart_subtotal * v_coupon.discount_value / 100, COALESCE(v_coupon.max_discount_amount, p_cart_subtotal))::DECIMAL(10,2);
    ELSE
        RETURN QUERY SELECT TRUE, NULL::VARCHAR(50), NULL::TEXT, 
            LEAST(v_coupon.discount_value, p_cart_subtotal)::DECIMAL(10,2);
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Função para calcular totais do carrinho
CREATE OR REPLACE FUNCTION cart.calculate_cart_totals(p_cart_id UUID)
RETURNS TABLE (
    item_count INT,
    subtotal DECIMAL(10, 2),
    total_items INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::INT AS item_count,
        COALESCE(SUM(ci.unit_price * ci.quantity), 0)::DECIMAL(10, 2) AS subtotal,
        COALESCE(SUM(ci.quantity), 0)::INT AS total_items
    FROM cart.items ci
    WHERE ci.cart_id = p_cart_id
    AND ci.removed_at IS NULL;
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- PARTE 14: SEED DATA - Roles padrão
-- ========================================

-- Roles padrão do sistema (managed by EF Core Migrations)
/*
INSERT INTO users.asp_net_roles (id, name, normalized_name, concurrency_stamp)
VALUES
    (uuid_generate_v4(), 'Admin', 'ADMIN', uuid_generate_v4()::TEXT),
    (uuid_generate_v4(), 'Customer', 'CUSTOMER', uuid_generate_v4()::TEXT),
    (uuid_generate_v4(), 'Manager', 'MANAGER', uuid_generate_v4()::TEXT)
ON CONFLICT DO NOTHING;
*/

-- ========================================
-- PARTE 15: COMMENTS
-- ========================================

-- Users (managed by EF Core Migrations)
COMMENT ON SCHEMA users IS 'Módulo de usuários: autenticação, perfis, endereços, sessões';
/*
COMMENT ON TABLE users.asp_net_users IS 'Usuários gerenciados pelo ASP.NET Identity';
COMMENT ON TABLE users.asp_net_roles IS 'Papéis/perfis de acesso do sistema';
COMMENT ON TABLE users.asp_net_user_roles IS 'Relacionamento entre usuários e roles';
COMMENT ON TABLE users.asp_net_user_claims IS 'Permissões específicas por usuário';
COMMENT ON TABLE users.asp_net_role_claims IS 'Permissões atribuídas a roles';
COMMENT ON TABLE users.asp_net_user_logins IS 'Provedores externos de autenticação';
COMMENT ON TABLE users.asp_net_user_tokens IS 'Tokens para recuperação de senha, confirmação de email, etc';
COMMENT ON TABLE users.profiles IS 'Dados estendidos do perfil do usuário';
COMMENT ON TABLE users.addresses IS 'Endereços de entrega e cobrança';
COMMENT ON TABLE users.sessions IS 'Sessões ativas para gerenciamento de dispositivos';
COMMENT ON TABLE users.notifications IS 'Notificações in-app';
*/

-- Catalog
COMMENT ON SCHEMA catalog IS 'Módulo de catálogo: produtos, categorias, marcas, estoque, avaliações';
COMMENT ON TABLE catalog.brands IS 'Marcas de produtos (Nike, Adidas, etc)';
COMMENT ON TABLE catalog.categories IS 'Categorias hierárquicas de produtos';
COMMENT ON TABLE catalog.products IS 'Catálogo de produtos';
COMMENT ON TABLE catalog.product_images IS 'Imagens dos produtos';
COMMENT ON TABLE catalog.stock_movements IS 'Histórico de movimentações de estoque';
COMMENT ON TABLE catalog.stock_reservations IS 'Reservas temporárias de estoque';
COMMENT ON TABLE catalog.product_reviews IS 'Avaliações de produtos';
COMMENT ON TABLE catalog.user_favorites IS 'Produtos favoritos dos usuários';

-- Cart
COMMENT ON SCHEMA cart IS 'Módulo de carrinho de compras';
COMMENT ON TABLE cart.carts IS 'Carrinhos de compras (logados e anônimos)';
COMMENT ON TABLE cart.items IS 'Itens do carrinho com snapshot do produto';
COMMENT ON TABLE cart.activity_log IS 'Log de atividades para analytics';
COMMENT ON TABLE cart.saved_carts IS 'Carrinhos salvos para compra futura';

-- Orders
COMMENT ON SCHEMA orders IS 'Módulo de pedidos: pedidos, itens, rastreamento, NFs';
COMMENT ON TABLE orders.orders IS 'Pedidos dos clientes';
COMMENT ON TABLE orders.items IS 'Itens do pedido com snapshot do produto';
COMMENT ON TABLE orders.status_history IS 'Histórico completo de mudanças de status';
COMMENT ON TABLE orders.tracking_events IS 'Eventos de rastreamento da transportadora';
COMMENT ON TABLE orders.invoices IS 'Notas fiscais emitidas';
COMMENT ON TABLE orders.refunds IS 'Reembolsos de pedidos';

-- Payments
COMMENT ON SCHEMA payments IS 'Módulo de pagamentos: transações, métodos, chargebacks';
COMMENT ON TABLE payments.user_methods IS 'Métodos de pagamento salvos (tokenizados)';
COMMENT ON TABLE payments.payments IS 'Pagamentos processados';
COMMENT ON TABLE payments.transactions IS 'Operações individuais com o gateway';
COMMENT ON TABLE payments.refunds IS 'Reembolsos processados';
COMMENT ON TABLE payments.chargebacks IS 'Contestações/estornos de cartão';
COMMENT ON TABLE payments.webhooks IS 'Webhooks recebidos dos gateways';

-- Coupons
COMMENT ON SCHEMA coupons IS 'Módulo de cupons de desconto';
COMMENT ON TABLE coupons.coupons IS 'Cupons de desconto';
COMMENT ON TABLE coupons.eligible_categories IS 'Categorias elegíveis para o cupom';
COMMENT ON TABLE coupons.eligible_products IS 'Produtos elegíveis para o cupom';
COMMENT ON TABLE coupons.eligible_users IS 'Usuários elegíveis para cupons exclusivos';
COMMENT ON TABLE coupons.usages IS 'Registro de uso dos cupons';
COMMENT ON TABLE coupons.reservations IS 'Reservas temporárias durante checkout';

-- Shared
COMMENT ON SCHEMA shared IS 'Infraestrutura compartilhada: eventos, auditoria';
COMMENT ON TABLE shared.domain_events IS 'Outbox pattern para eventos de domínio';
COMMENT ON TABLE shared.processed_events IS 'Inbox para idempotência';
COMMENT ON TABLE shared.audit_logs IS 'Log de auditoria unificado';

-- ========================================
-- FIM DO SCRIPT
-- ========================================

